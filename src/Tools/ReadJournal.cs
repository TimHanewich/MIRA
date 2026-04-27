using System;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;

namespace AIA
{
    public class ReadJournal : ExecutableFunction
    {
        State UseState {get; set;}

        public ReadJournal(State use_state)
        {
            Name = "read_journal";
            Description = "Read your investment journal to review past entries, strategies, and thoughts you previously logged.";

            UseState = use_state;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            //Check if there are entries
            if (UseState.InvestmentJournal.Count == 0)
            {
                return "Your investment journal is empty. No entries have been logged yet.";
            }

            //Compile entries
            string toreturn = "Your investment journal has " + UseState.InvestmentJournal.Count.ToString() + " entries:\n\n";
            foreach (JournalEntry je in UseState.InvestmentJournal)
            {
                toreturn += je.EnteredAt.ToString("yyyy-MM-dd HH:mm:ss") + " - " + je.Entry + "\n";
            }

            return toreturn;
        }
    }
}
