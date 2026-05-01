using System;
using TheMotleyFool.Transcripts;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;

namespace AIA
{
    public class Prompt
    {
        public TimHanewich.Investing.Simulation.Portfolio Portfolio {get; set;}
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
            prompt.Add("The current date and time is " + DateTimeOffset.Now.ToString() + ".");
            prompt.Add("You have been awoken to review your portfolio, analyze the market, and be given the opportunity to make changes as you see fit (or none if you see fit).");
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

            //SEC EDGAR info
            prompt.Add("You also have the ability to access financial filing data from the SEC's EDGAR database. A common flow will look like this:");
            prompt.Add("1. Use your `get_cik` tool to find the CIK for any publicly traded company from its ticker symbol. Use this if you do not know the CIK already.");
            prompt.Add("2. Use your `search_financial_data` tool to search through the available financial data points that company has previously reported and you can access.");
            prompt.Add("3. Use your `get_financial_data` to access data for one of those returned facts, showing you the historical data points for that fact for that company.");
            prompt.Add("");

            //Encouragement: ways to go about
            prompt.Add("Some good ideas you may want to use:");
            prompt.Add("Use your web_search tool to perform prospective investment research online.");
            prompt.Add("Read the latest Earnings Call Transcripts to identify new trends/news/information that may lead to a trading strategy. You can use the `read_earnings_call_transcript` tool to do so, but must provide it with a URL to the transcript hosted at fool.com. Use your `web_search` tool to find these transcripts as appropriate.");
            prompt.Add("For making longer-term, value-based investments, it is a good idea to actually review the company's historical financial data. Use the `search_financial_data` and `get_financial_data` tools for querying data from the company's previously reported 10-K's and 10-Q's.");
            prompt.Add("");

            //Must make decisions all right now
            prompt.Add("Do not respond back to me until you have finished for the day. As soon as you respond back to me, I'll assume you are complete and put you back to sleep until next time.");
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