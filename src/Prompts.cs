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
                prompt.Add("(no journal entries made");
            }
            else
            {
                foreach (JournalEntry je in journal_entries)
                {
                    prompt.Add("On " + je.EnteredAt.ToString() + ": " + je.Entry);
                }
            }
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