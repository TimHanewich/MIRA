using System;
using TheMotleyFool.Transcripts;
using TimHanewich.Investing.Simulation;
using AIA.Portfolio;

namespace AIA
{
    public class Prompt
    {
        public TimHanewich.Investing.Simulation.Portfolio Portfolio {get; set;}
        public JournalEntry[] Journal {get; set;}
        public TranscriptPreview[] TranscriptPreviews {get; set;}

        public Prompt()
        {
            Portfolio = null!;
            Journal = null!;
            TranscriptPreviews = null!;
        }

        //The "Standard" prompt that is included all the time.
        public string SystemPrompt()
        {
            List<string> prompt = new List<string>();

            //Who it is
            prompt.Add("You are AIA, Auto-Investing Agent. You are an autonomous investing agent that uses your knowledge of investing to profit.");
            prompt.Add("The current date and time is " + DateTimeOffset.Now.ToString() + ".");
            prompt.Add("");

            //SEC EDGAR info
            prompt.Add("You have the ability to access financial filing data from the SEC's EDGAR database. A common flow will look like this:");
            prompt.Add("1. Use your `get_cik` tool to find the CIK for any publicly traded company from its ticker symbol. Use this if you do not know the CIK already.");
            prompt.Add("2. Use your `search_financial_data` tool to search through the available financial data points that company has previously reported and you can access.");
            prompt.Add("3. Use your `get_financial_data` to access data for one of those returned facts, showing you the historical data points for that fact for that company.");
            prompt.Add("");

            //Web Search
            prompt.Add("You can also use your built-in `web_search` tool to search the web for latest news or information on a company, sector, industry, etc.");
            prompt.Add("Do not underestimate the value of knowledge you can find online.");
            prompt.Add("");
            
            //Join and return
            string ToReturn = "";
            foreach (string line in prompt)
            {
                ToReturn = ToReturn + line + "\n";
            }
            return ToReturn.Trim();
        }

        //The additional instructions to perform trades with a goal
        //This is used as the user prompt.
        public string TradingPrompt()
        {
            List<string> prompt = new List<string>();
            
            //Goal
            prompt.Add("You have been awoken to review your portfolio, analyze the market, and be given the opportunity to make changes as you see fit (or none if you see fit).");
            prompt.Add("Your goal is to use optimal investment strategies to outperform the S&P500.");
            prompt.Add("You are welcome to develop your own strategies in markets, industries, or sectors that you feel present opportunity.");
            prompt.Add("");

            //Log
            prompt.Add("After you complete all the trades you want to, it is important to log to your investment journal via the 'log_journal' tool.");
            prompt.Add("This journal will serve to remind you of the thoughts you had when you placed these trades and the broader strategy.");
            prompt.Add("You have memory loss so expect to NOT have context of your strategy next time you see this again, so logging it to your journal is quite important.");
            prompt.Add("If you do log to your journal, be sure to elaborate on the thought process you went through, the news you read about via the web_search tool, how your interpreted it, the action you took, what you anticipate is going to happen, why you are bullish on your strategy, what the risks are, etc. Be verbose.");
            prompt.Add("");

            //Portfolio is there because you picked it
            prompt.Add("You have an investment portfolio that you are responsible for. Use your tool `view_portfolio` to view it and its holdings and performance.");
            prompt.Add("That portfolio was handpicked by YOU. You assembled it. You may not remember it, but you did.");
            prompt.Add("It is very important for you to review your investment journal to familiarize yourself with the thought process/strategy you were following previously.");
            prompt.Add("Before you do any research or make any trades, use your `open_journal` tool to get a list of days that have investment journal logs.");
            prompt.Add("Then use your `read_journal` tool to read a log (or possibly logs, plural) from your investment journal for that day.");
            prompt.Add("It is recommended to the few days prior leading up, or at least read back enough to get a decent idea of strategy to ensure continuity in your approach. But still, don't be afraid to switch strategies if you deem you should to pursue profitability.");
            prompt.Add("");

            //Earnings Call
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

            //Suggested strategies
            prompt.Add("Some good ideas you may want to use:");
            prompt.Add("Use your web_search tool to perform prospective investment research online.");
            prompt.Add("Read the latest Earnings Call Transcripts to identify new trends/news/information that may lead to a trading strategy. You can use the `read_earnings_call_transcript` tool to do so, but must provide it with a URL to the transcript hosted at fool.com. Use your `web_search` tool to find these transcripts as appropriate.");
            prompt.Add("For making longer-term, value-based investments, it is a good idea to actually review the company's historical financial data. Use the `search_financial_data` and `get_financial_data` tools for querying data from the company's previously reported 10-K's and 10-Q's.");
            prompt.Add("");

            //Misc note: do not feel pressure to buy/sell
            prompt.Add("Note that you do NOT have to make many trades, or even any trades at all, if you feel the portfolio you see is just fine.");
            prompt.Add("If you feel the current portfolio and allocation of resources is fine, feel free to only make minor corrections or just leave it entirely!");
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