using System;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;

namespace MIRA
{
    public class ReadJournal : ExecutableFunction
    {
        State UseState {get; set;}

        public ReadJournal(State use_state)
        {
            Name = "read_journal";
            Description = "Read investment log(s) from a particular day.";
            InputParameters.Add(new TimHanewich.Foundry.OpenAI.Responses.FunctionInputParameter("date", "The date to read from, in MM/DD/YYYY format."));

            UseState = use_state;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get date
            JProperty? prop_date = arguments.Property("date");
            if (prop_date == null)
            {
                return "Must provide argument 'date'";
            }
            string datestr = prop_date.Value.ToString();

            //try parsing
            DateTime date;
            try
            {
                date = DateTime.Parse(datestr);
            }
            catch
            {
                return "Unable to parse '" + datestr + "' into a DateTime.";
            }

            //Get logs
            List<JournalEntry> LogsToReturn = new List<JournalEntry>();
            foreach (JournalEntry je in UseState.InvestmentJournal)
            {
                if (je.EnteredAt.Year == date.Year && je.EnteredAt.Month == date.Month && je.EnteredAt.Day == date.Day)
                {
                    LogsToReturn.Add(je);
                }
            }

            //Put into string
            if (LogsToReturn.Count == 0)
            {
                return "No logs found for " + date.ToShortDateString();
            }
            else
            {
                string ToReturn = "";
                foreach (JournalEntry je in LogsToReturn)
                {
                    ToReturn = ToReturn + "## Journal Entry from " + je.EnteredAt.ToString() + ": " + "\n" + je.Entry + "\n\n\n";
                }   
                return ToReturn.Trim();
            }
        }
    }
}
