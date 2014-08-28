﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using POESKillTree.ViewModels;

namespace POESKillTree.Model
{
    public class PersistentData
    {
        public Options Options { get; set; }
        public List<PoEBuild> Builds { get; set; }

        public List<ListViewItem> BuildsAsListViewItems
        {
            get { return Builds.Select(b => new ListViewItem {Content = b}).ToList(); }
        }

        public void SavePersistentDataToFile()
        {
            var writer = new XmlSerializer(typeof (PersistentData));
            var file = new StreamWriter(@"PersistentData.xml");
            writer.Serialize(file, this);
            file.Close();
        }

        public void LoadPersistentDataFromFile()
        {
            if (File.Exists("PersistentData.xml"))
            {
                var reader = new StreamReader(@"PersistentData.xml");
                var ser = new XmlSerializer(typeof (PersistentData));
                var obj = (PersistentData)ser.Deserialize(reader);
                Options = obj.Options;
                reader.Close();
            }
        }

        public void SaveBuilds(ItemCollection items)
        {
            Builds = (from ListViewItem item in items select (PoEBuild) item.Content).ToList();
            SavePersistentDataToFile();
        }
    }
}
