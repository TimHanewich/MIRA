using System;
using Newtonsoft.Json;

namespace MIRA
{
    public class MIRASettings
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

        public static MIRASettings Load()
        {
            if (System.IO.File.Exists(SavePath))
            {
                string content = System.IO.File.ReadAllText(SavePath);
                MIRASettings? loaded = JsonConvert.DeserializeObject<MIRASettings>(content);
                if (loaded != null)
                {
                    return loaded;
                }
            }
            return new MIRASettings();
        }

        public void Save()
        {
            string content = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(SavePath, content);
        }

    }
}