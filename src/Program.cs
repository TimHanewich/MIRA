using System;
using TimHanewich.Investing.Simulation;
using TimHanewich.AgentFramework;
using Spectre.Console;
using TimHanewich.Foundry;
using Newtonsoft.Json.Linq;
using TheMotleyFool.Transcripts;
using Newtonsoft.Json;
using SecuritiesExchangeCommission.Edgar.Data;
using SecuritiesExchangeCommission.Edgar;
using AIA.YFinanceServer;
using AIA.Portfolio;

namespace AIA
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            EntryPoint().Wait();
        }

        public static async Task EntryPoint()
        {
            AnsiConsole.MarkupLine("[bold][underline]AIA: Auto Invest Agent[/][/]");
            AnsiConsole.MarkupLine("Config Dir: " + Tools.ConfigDirectoryPath);
            
            //Ask what to do
            Console.WriteLine();
            SelectionPrompt<string> SelectToDo = new SelectionPrompt<string>();
            SelectToDo.AddChoice("Run AIA (timer)");
            SelectToDo.AddChoice("Wake up right now");
            SelectToDo.AddChoice("Review Portfolio");
            string selection = AnsiConsole.Prompt(SelectToDo);

            //Handle what to do
            if (selection == "Run AIA (timer)")
            {
                await EnterAsync();
            }
            else if (selection == "Wake up right now")
            {
                await WakeAsync();
            }
            else if (selection   == "Review Portfolio")
            {
                //Load state
                AnsiConsole.Markup("Loading State from storage... ");
                State state = State.Load();
                AnsiConsole.MarkupLine("[green]loaded[/]");

                //Gather current portfolio value and such
                AnsiConsole.Markup("Gathering portfolio performance details for " + state.Portfolio.Holdings().Length.ToString() + " holdings... ");
                DateTimeOffset pp_start_at = DateTimeOffset.Now;
                PortfolioDashboard pd = await PortfolioDashboard.ConstructAsync(state.Portfolio);
                DateTimeOffset pp_end_at = DateTimeOffset.Now;
                AnsiConsole.MarkupLine("[green]done after " + (pp_end_at - pp_start_at).TotalSeconds.ToString("#,##0") + " seconds[/]");
                Console.WriteLine();

                //Print
                AnsiConsole.MarkupLine("[bold][underline][blue]AIA Managed Portfolio[/][/][/]");
                AnsiConsole.MarkupLine("Cash Balance: $" + state.Portfolio.Cash.ToString("#,##0.00"));
                AnsiConsole.MarkupLine("Trades made: " + state.Portfolio.HoldingTransactionLog.Count.ToString("#,##0"));
                Console.WriteLine();

                //Print total profitability
                AnsiConsole.MarkupLine("[underline]Profitability[/]");
                if (pd.TotalGainLoss > 0.0f)
                {
                   AnsiConsole.MarkupLine("Net Profit: [green][bold]$" + Math.Abs(pd.TotalGainLoss).ToString("#,##0") + "[/][/]"); 
                }
                else
                {
                    AnsiConsole.MarkupLine("Net Loss: [red][bold]$" + Math.Abs(pd.TotalGainLoss).ToString("#,##0") + "[/][/]"); 
                }
                Console.WriteLine();

                //Sort holding performances by gain (return) from most to least
                PortfolioHolding[] sorted_phs = pd.Holdings.OrderByDescending(hp => hp.UnrealizedGainLoss).ToArray();

                //Print holdings and performances
                AnsiConsole.MarkupLine("[underline]Holdings[/]");
                foreach (PortfolioHolding ph in sorted_phs)
                {
                    AnsiConsole.Markup("[bold]" + ph.Symbol + "[/]: " + "[blue]" + ph.QuantityOwned.ToString("#,##0") + " shares[/], ");
                    
                    //Determine color
                    string color = "";
                    if (ph.UnrealizedGainLoss > 0.0f)
                    {
                        color = "green";
                    }
                    else
                    {
                        color = "red";
                    }

                    //Print
                    AnsiConsole.MarkupLine("[" + color + "]" + ph.UnrealizedGainLossPercent.ToString("#0.0") + "%[/], [" + color + "]$" + ph.UnrealizedGainLoss.ToString("#,##0") + "[/]");
                }
                
            }
            else
            {
                AnsiConsole.MarkupLine("[red]That command isn't handled yet.[/]");
            }
        }

        public static async Task EnterAsync()
        {
            //Welcome message
            AnsiConsole.MarkupLine("[bold][blue]WELCOME TO AIA:[/][/]");
            AnsiConsole.MarkupLine("[bold][blue]Auto-Invest Agent[/][/]");
            AnsiConsole.MarkupLine("[italic]" + "github.com/TimHanewich" + "[/]");
            Console.WriteLine();

            //Validate settings
            AnsiConsole.MarkupLine("[underline]Validating Settings[/]");
            AnsiConsole.Markup("Loading settings... ");
            AIASettings settings = AIASettings.Load();
            AnsiConsole.MarkupLine("loaded!");
            if (settings.FoundryEndpoint != null && settings.FoundryApiKey != null && settings.FoundryModel != null)
            {
                AnsiConsole.MarkupLine("[green]Settings populated![/]");
                Console.WriteLine();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Settings not populated! Please update settings at " + AIASettings.SavePath + ".[/]");
                return;
            }

            //Continuously wake
            while (true)
            {

                //Get next wake time
                DateTimeOffset NextWakeTime = Tools.NextWakeTime();

                //Count down until then
                while (true)
                {
                    TimeSpan UntilNextWakeTime = NextWakeTime - DateTimeOffset.Now;
                    string ToPrint = "\rWaiting until " + NextWakeTime.Month.ToString() + "/" + NextWakeTime.Day.ToString() + "/" + NextWakeTime.Year.ToString() + " at 3 PM EST: " + UntilNextWakeTime.TotalSeconds.ToString("#,##0") + " seconds remaining...";
                    while (ToPrint.Length < Console.WindowWidth)
                    {
                        ToPrint = ToPrint + " ";
                    }
                    Console.Write(ToPrint);
                    await Task.Delay(1_000); // wait one second
                    if (UntilNextWakeTime.TotalSeconds <= 0)
                    {
                        break;
                    }
                }

                //Wake!
                await WakeAsync();
            }
        }

        public static async Task WakeAsync()
        {
            WakeUp:

            //Print awake
            DateTimeOffset WakeUpTime = DateTimeOffset.Now;
            AnsiConsole.MarkupLine("[bold]AIA WAKING UP AT " + DateTimeOffset.Now.ToString() + "[/]");

            //Load settings
            AnsiConsole.Markup("Loading settings... ");
            AIASettings settings = AIASettings.Load();
            AnsiConsole.MarkupLine("[green]loaded[/]");

            //Validate settings
            AnsiConsole.Markup("Validating settings... ");
            if (settings.FoundryEndpoint != null && settings.FoundryApiKey != null && settings.FoundryApiKey != null)
            {
                AnsiConsole.MarkupLine("[green]validated[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Settings arent filled in![/]");
                return;
            }

            //Setup YFinanceServerBridge
            YFinanceServerBridge yfsb = new YFinanceServerBridge();
            AnsiConsole.Markup("Confirming YFinance-Server online... ");
            bool online = await yfsb.AliveAsync();
            AnsiConsole.MarkupLine("[green]complete[/]");
            if (online == false)
            {
                AnsiConsole.MarkupLine("[red]yfinance-server is not online. This is needed for stock quotes. Please start this and try again.[/]");
                return;
            }

            //Set up SEC EDGAR
            AnsiConsole.Markup("Setting up SEC EDGAR infa... ");
            IdentificationManager.Instance.AppName = "AIA";
            IdentificationManager.Instance.AppVersion = "1.0";
            IdentificationManager.Instance.Email = "surferdude101@gmail.com";
            SECBandwidthManager bwm = new SECBandwidthManager();
            AnsiConsole.MarkupLine("[green]done[/]");

            //Load state
            AnsiConsole.Markup("Loading State from storage... ");
            State state = State.Load();
            AnsiConsole.MarkupLine("[green]loaded[/]");

            //Check if portfolio is brand new (i.e. hasn't even been started yet)
            AnsiConsole.Markup("Checking if portfolio is new... ");
            if (state.Portfolio.CashTransactionLog.Count == 0) //if no cash transaction logs, that means cash never got in either. It is new!
            {
                AnsiConsole.Markup("it is new. Seeding with money... ");
                state.Portfolio.EditCash(100_000.00f, CashTransactionType.Edit); //add in $100,000
                
                AnsiConsole.MarkupLine("[green]done[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[green]portfolio with " + state.Portfolio.Holdings().Length.ToString("#,##0") + " holdings loaded[/]");
            }
          
            //Gather current portfolio value and such
            AnsiConsole.Markup("Gathering portfolio performance details for " + state.Portfolio.Holdings().Length.ToString() + " holdings... ");
            DateTimeOffset pp_start_at = DateTimeOffset.Now;
            PortfolioDashboard? pd = null;
            while (pd == null)
            {
                try
                {
                    pd = await PortfolioDashboard.ConstructAsync(state.Portfolio);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[red]Failure while constructing Portfolio Dashboard: " + Markup.Escape(ex.Message) + "[/]");
                }

                //If not collected wait
                if (pd == null)
                {
                    AnsiConsole.Markup("[red]Was unable to construct Portfolio Dashboard. Waiting 3 minutes and then will try again. [/]");
                    await Task.Delay(new TimeSpan(0, 3, 0)); //3 mins
                    AnsiConsole.MarkupLine("ready.");
                }
            }
            DateTimeOffset pp_end_at = DateTimeOffset.Now;
            AnsiConsole.MarkupLine("[green]done after " + (pp_end_at - pp_start_at).TotalSeconds.ToString("#,##0") + " seconds[/]");

            //Gather latest earnings call transcripts available
            AnsiConsole.Markup("Gathering latest earnings calls... ");
            TranscriptSource ts = new TranscriptSource();
            TranscriptPreview[] previews = await ts.GetRecentTranscriptPreviewsNextPageAsync();
            AnsiConsole.MarkupLine("[green]" + previews.Length.ToString() + " collected[/]");

            //Construct prompt
            Prompt prompt = new Prompt();
            prompt.Portfolio = state.Portfolio;
            prompt.PortfolioDasboard = pd;
            prompt.Journal = state.InvestmentJournal.ToArray();
            prompt.TranscriptPreviews = previews;
            string prompt_str = prompt.ConstructPrompt();
            
            //Create the agentf
            FoundryResource fr = new FoundryResource(settings.FoundryEndpoint);
            fr.ApiKey = settings.FoundryApiKey;
            Agent AIA = new Agent(prompt_str);
            AIA.FoundryResource = fr;
            AIA.Model = settings.FoundryModel;
            AIA.WebSearchEnabled = true;
            AIA.ReasoningEffortLevel = TimHanewich.Foundry.OpenAI.Responses.ReasoningEffortLevel.High;
            AIA.InferenceRequested += OnThinking;
            AIA.ExecutableFunctionInvoked += OnFunctionInvoked;
            AIA.WebSearchInvoked += OnWebSearchInvoked;
            AIA.WebSearchPageOpened += OnWebpageOpened;
            
            //Register tools
            AIA.Tools.Add(new Quote());
            AIA.Tools.Add(new Buy(state));
            AIA.Tools.Add(new Sell(state));
            AIA.Tools.Add(new LogJournal(state));
            AIA.Tools.Add(new ReadJournal(state));
            AIA.Tools.Add(new ReadEarningsCallTranscript());
            AIA.Tools.Add(new GetCIK());                            //Get the SEC CIK for a company from the stock symbol
            AIA.Tools.Add(new SearchFinancialData(bwm));            //Search available financial facts the company has previously reported
            AIA.Tools.Add(new GetFinancialData(bwm));               //Get one of those financial facts

            //Prompt it
            DateTimeOffset InferenceBegan = DateTimeOffset.Now;
            AnsiConsole.MarkupLine("NOW RUNNING AGENT!");
            int TradeLimit = 10; //the limit number of trades this agent can make in this one turn
            string? response = null;
            try
            {
                response = await AIA.PromptAsync("Please proceed with your goal. You can make up to " + TradeLimit.ToString() + " trades. Go! And Good luck.");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Prompt failed! Msg: " + Markup.Escape(ex.Message) + "[/]");
            }
            DateTimeOffset InferenceEnded = DateTimeOffset.Now;
        
            //Print response
            if (response != null) //SOME response came through
            {
                Console.WriteLine();
                AnsiConsole.MarkupLine("[blue]" + Markup.Escape(response) + "[/]");
                Console.WriteLine();

                //If it was blocked (response was "I'm sorry, but I cannot assist with that request"), 
                //that means Foundry guardrails blocked it. It is likely something it "saw" during the web searches (or got into the EC transcripts)
                // causd it to be blocked
                if (response.ToLower().Contains("i cannot assist with that request"))
                {
                    AnsiConsole.Markup("[DarkOrange]Block Detected. Will throw away this session and try again in 3 minutes. [/]");
                    await Task.Delay(1_000 * 60 * 3); //3 mins
                    AnsiConsole.MarkupLine("ready.");
                    goto WakeUp;
                }
            }
            else // no response came through... it failed (catch bracket)
            {
                AnsiConsole.MarkupLine("[red]Failure while prompting. Unsuccessful run. Will throw away this session and try again in 10 minutes.[/]");
                await Task.Delay(1_000 * 60 * 10); //10 mins
                goto WakeUp;
            }

            //Increment counters
            state.InputTokensConsumed = state.InputTokensConsumed + AIA.InputTokensConsumed;
            state.OutputTokensConsumed = state.OutputTokensConsumed + AIA.OutputTokensConsumed;
            
            //Stats
            TimeSpan AwakeFor = DateTimeOffset.UtcNow - WakeUpTime;
            TimeSpan InferenceTime = InferenceEnded - InferenceBegan;
            float TokensPerMinute = Convert.ToSingle(AIA.InputTokensConsumed + AIA.OutputTokensConsumed) / Convert.ToSingle(InferenceTime.TotalMinutes);
            AnsiConsole.MarkupLine("Ran for [bold]" + AwakeFor.TotalMinutes.ToString("#,##0.0") + " minutes[/], inference for [bold]" + InferenceTime.TotalMinutes.ToString("#,##0.0") + " minutes[/]: [bold]" + AIA.InputTokensConsumed.ToString("#,##0") + "[/] input tokens, [bold]" + AIA.OutputTokensConsumed.ToString("#,##0") + "[/] output tokens ([bold]" + TokensPerMinute.ToString("#,##0") + "[/] TPM)");

            //Save state!
            AnsiConsole.Markup("Saving state to storage... ");
            state.Save();
            AnsiConsole.MarkupLine("[green]saved![/]");

            //Shutdown
            AnsiConsole.MarkupLine("Going to sleep. Good bye.");
        }



        //Function invoked
        public static void OnFunctionInvoked(ExecutableFunction ef, JObject args)
        {
            AnsiConsole.MarkupLine("Calling '[bold]" + ef.Name + "[/]' [gray]with " + args.ToString(Newtonsoft.Json.Formatting.None) + "[/]... ");
        }

        //Web search invoked
        public static void OnWebSearchInvoked(string query)
        {
            AnsiConsole.MarkupLine("Web Searched '[bold]" + query + "[/]'");
        }

        //Web page opened
        public static void OnWebpageOpened()
        {
            AnsiConsole.MarkupLine("Read webpage.");
        }

        public static void OnThinking()
        {
            AnsiConsole.MarkupLine("[gray][italic]thinking...[/][/]");
        }


    }
}