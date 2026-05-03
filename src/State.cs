using System;
using TimHanewich.Investing.Simulation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MIRA
{
    public class State
    {
        public TimHanewich.Investing.Simulation.Portfolio Portfolio {get; set;}
        public List<JournalEntry> InvestmentJournal {get; set;}
        public int InputTokensConsumed {get; set;}
        public int OutputTokensConsumed {get; set;}

        public State()
        {
            Portfolio = new TimHanewich.Investing.Simulation.Portfolio();
            InvestmentJournal = new List<JournalEntry>();
        }

        public static string SavePath
        {
            get
            {
                return Path.Combine(Tools.ConfigDirectoryPath, "state.json");
            }
        }

        public static State Load()
        {
            if (System.IO.File.Exists(SavePath))
            {
                string content = System.IO.File.ReadAllText(SavePath);
                State? loaded = JsonConvert.DeserializeObject<State>(content);
                if (loaded != null)
                {
                    return loaded;
                }
            }

            //A local one did not load, so create a new one but also save to folder
            State NewToReturn = new State();
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