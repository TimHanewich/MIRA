using System;
using TheMotleyFool.Transcripts;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;

namespace AIA
{
    public class Prompt
    {
        public Portfolio Portfolio {get; set;}
        public PortflioPerformance PortfolioPerformance {get; set;}
        public JournalEntry[] Journal {get; set;}
        public TranscriptPreview[] TranscriptPreviews {get; set;}

        public Prompt()
        {
            Portfolio = null!;
            PortfolioPerformance = null!;
            Journal = null!;
            TranscriptPreviews = null!;
        }

        public string ConstructPrompt()
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
            prompt.Add(Portfolio.ToString());
            prompt.Add("");
            prompt.Add("This is your portfolio's current performance:");
            prompt.Add(PortfolioPerformance.ToString());
            prompt.Add("");

            //Log
            prompt.Add("After you complete all the trades you want to, it is important to log to your investment journal via the 'log_journal' tool.");
            prompt.Add("This journal will serve to remind you of the thoughts you had when you placed these trades and the broader strategy.");
            prompt.Add("You have memory loss so expect to NOT have context of your strategy next time you see this again, so logging it to your journal is quite important.");
            prompt.Add("If you do log to your journal, be sure to elaborate on the thought process you went through, the news you read about via the web_search tool, how your interpreted it, the action you took, what you anticipate is going to happen, why you are bullish on your strategy, what the risks are, etc. Be verbose.");
            prompt.Add("");

            //Add investment journal info
            prompt.Add("These are your investment journal entries, notes you have taken on previous days about strategy and your thoughts as time goes on:");
            if (Journal.Length == 0)
            {
                prompt.Add("(no journal entries made)");
            }
            else
            {
                foreach (JournalEntry je in Journal)
                {
                    prompt.Add("On " + je.EnteredAt.ToString() + ": " + je.Entry);
                }
            }
            prompt.Add("");

            //Today's earnings call
            if (TranscriptPreviews != null)
            {
                prompt.Add("Earnings Calls Today");
                prompt.Add("These are the Earnings Calls that happened today. If you wish, you can read the full transcript by requesting it via the 'read_earnings_call_transcript' tool.");
                foreach (TranscriptPreview tp in TranscriptPreviews)
                {
                    if (tp.PostedDate.Year == DateTime.Now.Year && tp.PostedDate.Month == DateTime.Now.Month && tp.PostedDate.Day == DateTime.Now.Day)
                    {
                        prompt.Add("- " + tp.Title + " (" + tp.Url + ")");
                    }
                }
                prompt.Add("");
            }

            //Encouragement
            prompt.Add("Some good ideas you may want to use:");
            prompt.Add("Use your web_search tool to perform prospective investment research online.");
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