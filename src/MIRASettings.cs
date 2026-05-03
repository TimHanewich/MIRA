using System;
using Newtonsoft.Json;

namespace MIRA
{
    public class MIRASettings
    {
        public string FoundryEndpoint {get; set;}
        public string FoundryApiKey {get; set;}
        public string FoundryModel {get; set;}

        public MIRASettings()
        {
            FoundryEndpoint = "<foundry endpoint here, such as 'https://myservice.services.ai.azure.com'>";
            FoundryApiKey = "<foundry API key here, such as 'CE6l5HWOMps0k8fhsFbCdWozJlhrjRdE6NCEqxvhGPRXSJQQJ99CDACHYHv6XJ3w3ADBDOG9VJq'>";
            FoundryModel = "<foundry model deployment name here, such as 'gpt-5-mini'>";
        }

        //Determines if the "default" fill in values have not yet been added
        //If only ONE of the fields is default, it will return true
        public bool IsDefault()
        {
            MIRASettings ToCompareTo = new MIRASettings();
            return FoundryEndpoint == ToCompareTo.FoundryEndpoint || FoundryApiKey == ToCompareTo.FoundryEndpoint || FoundryModel == ToCompareTo.FoundryModel;
        }

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