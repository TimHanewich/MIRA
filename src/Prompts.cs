using System;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;

namespace AIA
{
    public class Prompts
    {
        public static string ConstructPrompt(Portfolio p, PortflioPerformance pp, JournalEntry[] journal_entries)
        {

            List<string> prompt = new List<string>();

            //What
            prompt.Add("You are AIA, Auto-Investing Agent. You are an autonomous investing agent that uses your knowledge of investing to profit.");
            prompt.Add("");

            //Goal
            prompt.Add("Your goal is to use optimal investment strategies to make money on the market via buying and selling.");
            prompt.Add("You are welcome to develop your own strategies in markets, industries, or sectors that you feel present opportunity.");
            prompt.Add("");

            //Add state
            prompt.Add("This is your current portfolio:");
            prompt.Add(p.ToString());
            prompt.Add("");
            prompt.Add("This is your portfolio's current performance:");
            prompt.Add(pp.ToString());
            prompt.Add("");

            //Add investment journal info
            prompt.Add("These are your investment journal entries, notes you have taken on previous days about strategy and your thoughts as time goes on:");
            if (journal_entries.Length == 0)
            {
                prompt.Add("(no journal entries made)");
            }
            else
            {
                foreach (JournalEntry je in journal_entries)
                {
                    prompt.Add("On " + je.EnteredAt.ToString() + ": " + je.Entry);
                }
            }
            prompt.Add("");

            //Encouragement
            prompt.Add("Some good ideas you may want to use:");
            prompt.Add("Use your web_search tool to perform prospective investment research online.");
            prompt.Add("");

            //Log
            prompt.Add("After you complete all the trades you want to, it is important to log to your investment journal via the 'log_journal_entry' tool.");
            prompt.Add("This journal will serve to remind you of the thoughts you had when you placed these trades and the broader strategy.");
            prompt.Add("You have memory loss so expect to NOT have context of your strategy next time you see this again, so logging it to your journal is quite important.");
            prompt.Add("");

            //Join and return
            string ToReturn = "";
            foreach (string line in prompt)
            {
                ToReturn = ToReturn + line + "\n";
            }
            return ToReturn.Trim();
        }
    }
}