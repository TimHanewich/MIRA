using System;
using Newtonsoft.Json;

namespace AIA
{
    public class AIASettings
    {
        public string? FoundryEndpoint {get; set;}
        public string? FoundryApiKey {get; set;}
        public string? FoundryModel {get; set;}

        public static string SavePath
        {
            get
            {
                return Path.Combine(Tools.ConfigDirectoryPath, "settings.json");
            }
        }

        public static AIASettings Load()
        {
            if (System.IO.File.Exists(SavePath))
            {
                string content = System.IO.File.ReadAllText(SavePath);
                AIASettings? loaded = JsonConvert.DeserializeObject<AIASettings>(content);
                if (loaded != null)
                {
                    return loaded;
                }
            }
            return new AIASettings();
        }

        public void Save()
        {
            string content = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(SavePath, content);
        }

    }
}