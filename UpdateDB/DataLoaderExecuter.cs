﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils;
using UpdateDB.DataLoading;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB
{
    /// <summary>
    /// Runs <see cref="DataLoader"/> instances as specified via <see cref="IArguments"/>.
    /// </summary>
    public class DataLoaderExecutor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static readonly LoaderCollection LoaderDefinitions = new LoaderCollection
        {
            {"affixes", "Equipment/AffixList.xml", new AffixDataLoader(), LoaderCategories.VersionControlled, "Affixes"},
            {"base items", "Equipment/ItemList.xml", new ItemDataLoader(), LoaderCategories.VersionControlled, "Items"},
            {"base item images", "Equipment/Assets", new ItemImageLoader(false), LoaderCategories.NotVersionControlled, "Images"},
            {"skill tree assets", "", new SkillTreeLoader(), LoaderCategories.NotVersionControlled, "TreeAssets"},
            {"gems", "ItemDB/GemList.xml", new GemLoader(new GamepediaReader()), LoaderCategories.VersionControlled, "Gems"}
        };

        private readonly IArguments _arguments;

        private readonly string _savePath;

        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Creates an instance and sets it up using <paramref name="arguments"/>.
        /// </summary>
        /// <param name="arguments">The arguments that define how this instance behaves. Only
        /// <see cref="IArguments.OutputDirectory"/> is consumed in the constructor.</param>
        public DataLoaderExecutor(IArguments arguments)
        {
            _arguments = arguments;
            switch (arguments.OutputDirectory)
            {
                case OutputDirectory.AppData:
                    _savePath = AppData.GetFolder();
                    break;
                case OutputDirectory.SourceCode:
                    _savePath = Regex.Replace(Directory.GetCurrentDirectory(),
                        @"PoESkillTree((/|\\).*?)?$", "PoESkillTree/WPFSKillTree");
                    break;
                case OutputDirectory.Current:
                    _savePath = Directory.GetCurrentDirectory();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // necessary for SkillTreeLoader
            AppData.SetApplicationData(_savePath);
            _savePath = Path.Combine(_savePath, "Data");

            // The Affix file is big enough to be starved by other requests sometimes.
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        /// <summary>
        /// Returns true iff the given flag identifies a DataLoader (case-insensitive).
        /// </summary>
        public static bool IsLoaderFlagRecognized(string flag)
        {
            return LoaderDefinitions.Any(l => l.Flag.Equals(flag, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Runs all DataLoader instances asynchronously.
        /// </summary>
        /// <returns>A task that completes once all DataLoaders completed.</returns>
        public async Task LoadAllAsync()
        {
            Log.Info("Starting loading ...");
            Directory.CreateDirectory(_savePath);
            var explicitlyActivated = _arguments.LoaderFlags.ToList();
            var tasks = from loader in LoaderDefinitions
                        where loader.Category.HasFlag(_arguments.ActivatedLoaders)
                            || explicitlyActivated.Contains(loader.Flag)
                        select LoadAsync(loader.Name, loader.File, loader.DataLoader);
            await Task.WhenAll(tasks);
            Log.Info("Completed loading!");
        }

        private async Task LoadAsync(string name, string path, IDataLoader dataLoader)
        {
            Log.InfoFormat("Loading {0} ...", name);
            var fullPath = Path.Combine(_savePath, path);

            if (path.Any())
            {
                var isFolder = dataLoader.SavePathIsFolder;
                var tmpPath = fullPath + (isFolder ? "Tmp" : ".tmp");
                if (isFolder)
                {
                    Directory.CreateDirectory(tmpPath);
                }
                var task = dataLoader.LoadAndSaveAsync(_httpClient, tmpPath);

                if (_arguments.CreateBackup)
                    Backup(fullPath, isFolder);

                await task;
                MoveTmpToTarget(tmpPath, fullPath, isFolder);
            }
            else
            {
                // This is for SkillTreeLoader which has no dedicated file/folder and can't really be configured
                await dataLoader.LoadAndSaveAsync(_httpClient, fullPath);
            }
            Log.InfoFormat("Loaded {0}!", name);
        }

        private static void Backup(string path, bool isFolder)
        {
            if (isFolder && Directory.Exists(path))
            {
                var backupPath = path + "Backup";
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                Directory.CreateDirectory(backupPath);
                foreach (var filePath in Directory.GetFiles(path))
                {
                    File.Copy(filePath, filePath.Replace(path, backupPath), true);
                }
            }
            else if (!isFolder && File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }
        }

        private static void MoveTmpToTarget(string tmpPath, string targetPath, bool isFolder)
        {
            if (isFolder)
            {
                if (Directory.Exists(targetPath))
                    Directory.Delete(targetPath, true);
                Directory.Move(tmpPath, targetPath);
            }
            else
            {
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                File.Move(tmpPath, targetPath);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        /// <summary>
        /// Collection of <see cref="LoaderDefinition"/>s that supports easy initialization.
        /// </summary>
        private class LoaderCollection : IEnumerable<LoaderDefinition>
        {
            private readonly List<LoaderDefinition> _loaderDefinitions = new List<LoaderDefinition>();

            public void Add(string name, string file, IDataLoader dataLoader, LoaderCategories category, string flag)
            {
                _loaderDefinitions.Add(new LoaderDefinition
                {
                    Name = name,
                    File = file,
                    DataLoader = dataLoader,
                    Category = category,
                    Flag = flag
                });
            }

            public IEnumerator<LoaderDefinition> GetEnumerator()
            {
                return _loaderDefinitions.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        /// <summary>
        /// Defines a DataLoader.
        /// </summary>
        private class LoaderDefinition
        {
            /// <summary>
            /// The name that is used for console output.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The file/folder to which the loader saves its output.
            /// </summary>
            public string File { get; set; }
            /// <summary>
            /// The actual DataLoader instance.
            /// </summary>
            public IDataLoader DataLoader { get; set; }
            /// <summary>
            /// The category to which this loader belongs.
            /// </summary>
            public LoaderCategories Category { get; set; }
            /// <summary>
            /// A flag that identifies this loader.
            /// </summary>
            public string Flag { get; set; }
        }
    }
}