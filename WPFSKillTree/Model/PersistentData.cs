﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using POESKillTree.ViewModels;
using System.Collections.ObjectModel;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls;
using POESKillTree.Model.Items;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class PersistentData : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentData));

        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private List<StashBookmark> _stashBookmarks;
        public List<StashBookmark> StashBookmarks
        {
            get { return _stashBookmarks; }
            set { SetProperty(ref _stashBookmarks, value); }
        }

        private Options _options;
        public Options Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }

        private PoEBuild _currentBuild;
        public PoEBuild CurrentBuild
        {
            get { return _currentBuild; }
            set { SetProperty(ref _currentBuild, value); }
        }

        private List<PoEBuild> _builds;
        public List<PoEBuild> Builds
        {
            get { return _builds; }
            private set { SetProperty(ref _builds, value); }
        }

        private const string FileName = "PersistentData";

        private readonly ObservableCollection<Item> _stash = new ObservableCollection<Item>();
        [XmlIgnore]
        public ObservableCollection<Item> StashItems
        {
            get { return _stash; }
        }

        private readonly Lazy<EquipmentData> _equipmentData = new Lazy<EquipmentData>(() => new EquipmentData());
        [XmlIgnore]
        public EquipmentData EquipmentData
        {
            get { return _equipmentData.Value; }
        }

        public PersistentData()
        {
            Options = new Options();
            CurrentBuild = new PoEBuild
            {
                Name = "New Build",
                Url = SkillTree.TreeAddress + SkillTree.GetCharacterURL(3, 0),
                Level = "1"
            };
            Builds = new List<PoEBuild>();
        }

        // Creates empty file with language option set.
        public static void CreateSetupTemplate(string path, string language)
        {
            var data = new PersistentData {Options = {Language = language}};
            data.SavePersistentDataToFileEx(Path.Combine(path, FileName + ".xml"));
        }

        public void SavePersistentDataToFile()
        {
            SavePersistentDataToFileEx(AppData.GetFolder(true) + FileName + ".xml");
        }

        private void SavePersistentDataToFileEx(string path)
        {
            if (File.Exists(path))
            {
                string pathBak = AppData.GetFolder(true) + FileName + ".bak";
                if (File.Exists(pathBak))
                    File.Delete(pathBak);
                File.Move(path, pathBak);
            }
            var writer = new XmlSerializer(typeof(PersistentData));
            var file = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            writer.Serialize(file, this);
            file.Close();
            SerializeStash();
        }

        public void LoadPersistentDataFromFile()
        {
            LoadPersistenDataFromFileEx(AppData.GetFolder(true) + FileName + ".xml");
        }

        private void LoadPersistenDataFromFileEx(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var reader = new StreamReader(filePath);
                    var ser = new XmlSerializer(typeof(PersistentData));
                    var obj = (PersistentData)ser.Deserialize(reader);
                    Options = obj.Options;
                    Builds = obj.Builds;
                    CurrentBuild = obj.CurrentBuild;
                    StashBookmarks = obj.StashBookmarks;
                    AppVersion = obj.AppVersion;
                    reader.Close();
                }
                DeserializeStash();
            }
            catch (Exception ex)
            {
                string pathBak = AppData.GetFolder(true) + FileName + ".bak";
                if (!filePath.Contains(FileName + ".bak") && File.Exists(pathBak))
                    LoadPersistenDataFromFileEx(pathBak);
                else 
                {
                    string pathBad = AppData.GetFolder(true) + FileName + "_Bad.xml";
                    if(File.Exists(pathBad))
                        File.Delete(pathBad);
                    File.Copy(filePath, pathBad);
                    if (File.Exists(AppData.GetFolder(true) + FileName + ".xml"))
                        File.Delete(AppData.GetFolder(true) + FileName + ".xml");
                    if (File.Exists(AppData.GetFolder(true) + FileName + ".bak"))
                        File.Delete(AppData.GetFolder(true) + FileName + ".bak");

                    throw new Exception(ex.Message + "\nYour PersistentData folder could not be loaded correctly. It has been moved to " + pathBad);
                }
            }
        }

        private void SerializeStash()
        {
            try
            {
                var arr = new JArray();
                foreach (var item in StashItems)
                {
                    arr.Add(item.JsonBase);
                }

                File.WriteAllText(Path.Combine(AppData.GetFolder(), "stash.json"), arr.ToString());
            }
            catch (Exception e)
            {
                Log.Error("Could not serialize stash", e);
            }
        }

        private void DeserializeStash()
        {
            try
            {
                StashItems.Clear();
                var file = Path.Combine(AppData.GetFolder(), "stash.json");
                if (!File.Exists(file))
                    return;
                var arr = JArray.Parse(File.ReadAllText(file));
                foreach (var item in arr)
                {
                    var itm = new Item((JObject)item, EquipmentData);
                    StashItems.Add(itm);
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not deserialize stash", e);
            }
        }

        public void SetBuilds(ItemCollection items)
        {
            Builds = (from PoEBuild item in items select item).ToList();
        }
    }
}
