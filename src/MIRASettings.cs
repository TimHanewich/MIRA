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

            //A local one did not load, so create a new one but also save to folder
            MIRASettings NewToReturn = new MIRASettings();
            NewToReturn.Save();
            return NewToReturn;
        }

        public void Save()
        {
            string content = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(SavePath, content);
        }

    }
}