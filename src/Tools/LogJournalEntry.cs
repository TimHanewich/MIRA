using System;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;

namespace MIRA
{
    public class LogJournal : ExecutableFunction
    {
        State UseState {get; set;}

        public LogJournal(State use_state)
        {
            Name = "log_journal";
            Description = "Log a new entry to your investment journal that you can later review to remember the strategy you employed or throughts you had.";
            InputParameters.Add(new TimHanewich.Foundry.OpenAI.Responses.FunctionInputParameter("entry", "The journal entry."));

            UseState = use_state;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get entry
            JProperty? prop_entry = arguments.Property("entry");
            if (prop_entry == null)
            {
                return "Must provide parameter 'entry'.";
            }
            string entry = prop_entry.Value.ToString();

            //Add it
            UseState.InvestmentJournal.Add(new JournalEntry(entry));

            //Confirm
            return "Journal entry added.";
        }
    }
}