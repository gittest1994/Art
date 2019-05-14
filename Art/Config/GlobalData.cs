using HandyControl.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Art
{
    internal class GlobalData
    {
        internal class AppConfig
        {
            public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

            public string DataPath { get; set; } = Environment.CurrentDirectory + @"\data";
            public SkinType Skin { get; set; }
        }
        public static AppConfig Config { get; set; }

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(AppConfig.SavePath, json);
        }

        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    var json = File.ReadAllText(AppConfig.SavePath);
                    Config = JsonConvert.DeserializeObject<AppConfig>(json);
                }
                catch
                {
                    Config = new AppConfig();
                }
            }
            else
            {
                Config = new AppConfig();
            }
        }

    }
}
