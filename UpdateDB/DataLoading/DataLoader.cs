﻿using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace UpdateDB.DataLoading
{
    public interface IDataLoader
    {
        bool SavePathIsFolder { get; }

        Task LoadAndSaveAsync(HttpClient httpClient, string savePath);
    }

    public abstract class DataLoader : IDataLoader
    {
        protected string SavePath { get; private set; }

        public abstract bool SavePathIsFolder { get; }

        public async Task LoadAndSaveAsync(HttpClient httpClient, string savePath)
        {
            SavePath = savePath;
            await LoadAsync(httpClient);
            await CompleteSavingAsync();
        }

        /// <summary>
        /// Extracts data from the web.
        /// </summary>
        protected abstract Task LoadAsync(HttpClient httpClient);

        protected abstract Task CompleteSavingAsync();

        protected static int ParseInt(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }
        
        protected static bool TryParseInt(string s, out int i)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
        }

        protected static float ParseFloat(string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        protected static bool TryParseFloat(string s, out float f)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }
    }
}