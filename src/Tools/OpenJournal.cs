using System;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;

namespace AIA
{
    public class OpenJournal : ExecutableFunction
    {
        State UseState {get; set;}

        public OpenJournal(State use_state)
        {
            Name = "open_journal";
            Description = "Open your investment journal to review a list of past entries that can then be read.";

            UseState = use_state;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            //Check if there are entries
            if (UseState.InvestmentJournal.Count == 0)
            {
                return "Your investment journal is empty. No entries have been logged yet.";
            }

            //Sort journal entries
            UseState.InvestmentJournal = UseState.InvestmentJournal.OrderBy(j => j.EnteredAt).ToList();

            //Make list of dates
            List<string> dates = new List<string>();
            foreach (JournalEntry je in UseState.InvestmentJournal)
            {
                string ThisDate = je.EnteredAt.Month.ToString() + "/" + je.EnteredAt.Day.ToString() + "/" + je.EnteredAt.Year.ToString();
                if (dates.Contains(ThisDate) == false)
                {
                    dates.Add(ThisDate);
                }
            }

            //Compile entries
            string toreturn = "Your investment journal has entries from the following days:\n";
            foreach (string date in dates)
            {
                toreturn = toreturn + date + "\n";
            }

            return toreturn.Trim();
        }
    }
}
