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
using MIRA.YFinanceServer;

namespace MIRA
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            StartHereAsync().Wait();
        }

        public static async Task StartHereAsync()
        {
            //Set console tab name
            Console.Title = "💸 MIRA";

            string asciiart = @"
$$\      $$\       $$$$$$\       $$$$$$$\         $$$$$$\  
$$$\    $$$ |      \_$$  _|      $$  __$$\       $$  __$$\ 
$$$$\  $$$$ |        $$ |        $$ |  $$ |      $$ /  $$ |
$$\$$\$$ $$ |        $$ |        $$$$$$$  |      $$$$$$$$ |
$$ \$$$  $$ |        $$ |        $$  __$$<       $$  __$$ |
$$ |\$  /$$ |        $$ |        $$ |  $$ |      $$ |  $$ |
$$ | \_/ $$ |      $$$$$$\       $$ |  $$ |      $$ |  $$ |
\__|     \__|      \______|      \__|  \__|      \__|  \__|  
            ";

            Console.WriteLine(); //blank line
            AnsiConsole.Write(Align.Center(new Markup("[green]" + asciiart + "[/]")));
            AnsiConsole.Write(Align.Center(new Markup("[green][bold]MIRA[/]: [bold]M[/]arket [bold]I[/]ntelligence & [bold]R[/]esearch [bold]A[/]gent[/]")));
            AnsiConsole.Write(Align.Center(new Markup("[green]github.com/TimHanewich/MIRA[/]")));
            Console.WriteLine(); //blank line
            Console.WriteLine(); //blank line

            //Info
            AnsiConsole.MarkupLine("[bold][underline]Welcome to MIRA![/][/]");
            AnsiConsole.MarkupLine("[bold]MIRA[/] is an autonomous investing research agent that researches the market, develops strategies, and manages a persistent paper portfolio. [gray]Config Dir: " + Tools.ConfigDirectoryPath + "[/]");
            Console.WriteLine();

            //Validate settings
            AnsiConsole.MarkupLine("[underline]Validating Setup[/]");
            AnsiConsole.Markup("Loading settings... ");
            MIRASettings settings = MIRASettings.Load();
            AnsiConsole.MarkupLine("[green]loaded![/]");
            if (settings.FoundryEndpoint == null || settings.FoundryApiKey == null || settings.FoundryModel == null)
            {
                AnsiConsole.MarkupLine("[red]Settings not populated! Before you proceed, you must update settings at " + MIRASettings.SavePath + ".[/]");
                return;
            }

            //Validate YFinanceServerBridge
            YFinanceServerBridge yfsb = new YFinanceServerBridge();
            AnsiConsole.Markup("Confirming YFinance-Server online... ");
            bool online = await yfsb.AliveAsync();
            AnsiConsole.MarkupLine("[green]complete[/]");
            if (online == false)
            {
                AnsiConsole.MarkupLine("[red]yfinance-server is not online. This is needed for stock quotes. Please start this and try again.[/]");
                AnsiConsole.MarkupLine("[red]See the 'Getting Started' section of this project for more information.[/]");
                return;
            }
            Console.WriteLine();

            //Ask what to do
            Console.WriteLine();
            SelectionPrompt<string> SelectToDo = new SelectionPrompt<string>();
            SelectToDo.AddChoice("Schedule MIRA for Automated Investments");
            SelectToDo.AddChoice("Run MIRA Now: Autonomous Mode");
            SelectToDo.AddChoice("Run MIRA Now: Assistant Mode");
            SelectToDo.AddChoice("Review Portfolio");
            string selection = AnsiConsole.Prompt(SelectToDo);

            //Handle what to do
            if (selection == "Schedule MIRA for Automated Investments")
            {
                await PeriodicTradingAsync();
            }
            else if (selection == "Run MIRA Now: Autonomous Mode")
            {
                await WakeAsync(AgentMode.Autonomous, Tools.AskForCustomInstructions());
            }
            else if (selection == "Run MIRA Now: Assistant Mode")
            {
                await WakeAsync(AgentMode.Assistant);
            }
            else if (selection == "Review Portfolio")
            {
                await ReviewPortfolioAsync();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]That command isn't handled yet.[/]");
            }
        }

        public static async Task PeriodicTradingAsync()
        {
            //Ask for custom instruction input
            string? CustInstr = Tools.AskForCustomInstructions();
            Console.WriteLine();

            //Continuously wake
            while (true)
            {

                //Get next wake time
                DateTimeOffset NextWakeTime = Tools.NextWakeTime();

                //Count down until then
                while (true)
                {
                    TimeSpan UntilNextWakeTime = NextWakeTime - DateTimeOffset.Now;
                    string ToPrint = "\rWaiting until " + NextWakeTime.Month.ToString() + "/" + NextWakeTime.Day.ToString() + "/" + NextWakeTime.Year.ToString() + " at 3 PM EST to wake up: " + UntilNextWakeTime.TotalSeconds.ToString("#,##0") + " seconds remaining...";
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
                await WakeAsync(AgentMode.Autonomous, CustInstr);
            }
        }

        public static async Task WakeAsync(AgentMode mode, string? custom_instructions = null)
        {
            WakeUp:

            //Print awake
            DateTimeOffset WakeUpTime = DateTimeOffset.Now;
            AnsiConsole.MarkupLine("[bold]MIRA WAKING UP AT " + DateTimeOffset.Now.ToString() + "[/]");

            //Load settings
            AnsiConsole.Markup("Loading settings... ");
            MIRASettings settings = MIRASettings.Load();
            AnsiConsole.MarkupLine("[green]loaded[/]");

            //Set up SEC EDGAR
            AnsiConsole.Markup("Setting up SEC EDGAR infa... ");
            IdentificationManager.Instance.AppName = "MIRA";
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

            //Gather latest earnings call transcripts available
            AnsiConsole.Markup("Gathering latest earnings calls... ");
            TranscriptSource ts = new TranscriptSource();
            TranscriptPreview[] previews = await ts.GetRecentTranscriptPreviewsNextPageAsync();
            AnsiConsole.MarkupLine("[green]" + previews.Length.ToString() + " collected[/]");

            //Construct prompt
            Prompt prompt = new Prompt();
            string prompt_str = prompt.SystemPrompt();
            
            //Create the agent
            FoundryResource fr = new FoundryResource(settings.FoundryEndpoint);
            fr.ApiKey = settings.FoundryApiKey;
            Agent MIRA = new Agent(prompt_str);
            MIRA.FoundryResource = fr;
            MIRA.Model = settings.FoundryModel;
            MIRA.WebSearchEnabled = true;
            MIRA.ReasoningEffortLevel = TimHanewich.Foundry.OpenAI.Responses.ReasoningEffortLevel.High;
            MIRA.InferenceRequested += OnThinking;
            MIRA.ExecutableFunctionInvoked += OnFunctionInvoked;
            MIRA.WebSearchInvoked += OnWebSearchInvoked;
            MIRA.WebSearchPageOpened += OnWebpageOpened;
            
            //Register tools
            MIRA.Tools.Add(new Quote());
            MIRA.Tools.Add(new Buy(state));
            MIRA.Tools.Add(new Sell(state));
            MIRA.Tools.Add(new OpenJournal(state));                  //Get a list of journal entries on days
            MIRA.Tools.Add(new ReadJournal(state));                  //Open a particular day of journal logs
            MIRA.Tools.Add(new LogJournal(state));                   //Log to the investment journal
            MIRA.Tools.Add(new ReadEarningsCallTranscript());
            MIRA.Tools.Add(new GetCIK());                            //Get the SEC CIK for a company from the stock symbol
            MIRA.Tools.Add(new SearchFinancialData(bwm));            //Search available financial facts the company has previously reported
            MIRA.Tools.Add(new GetFinancialData(bwm));               //Get one of those financial facts
            MIRA.Tools.Add(new Calculate());                         //Math calculator
            MIRA.Tools.Add(new ViewPortfolio(state));                //Open portfolio

            //If in guided mode (continuous chat with user), hijack in infinite loop now.
            if (mode == AgentMode.Assistant)
            {
                Console.WriteLine();
                AnsiConsole.MarkupLine("[bold]----- AGENT CONVERSATION -----[/]");
                Console.WriteLine();

                while (true)
                {
                    //Collect input
                    string? input = null;
                    while (input == null || input == "")
                    {
                        AnsiConsole.Markup("[gray]\"/save\" to close[/] [bold]>[/] ");
                        input = Console.ReadLine();
                    }
                    
                    //if /save
                    if (input.ToLower().Trim() == "/save")
                    {
                        //Increment the state cumulative counters
                        state.InputTokensConsumed = state.InputTokensConsumed + MIRA.InputTokensConsumed;
                        state.OutputTokensConsumed = state.OutputTokensConsumed + MIRA.OutputTokensConsumed;

                        //Save and close
                        AnsiConsole.Markup("Saving state... ");
                        state.Save();
                        AnsiConsole.MarkupLine("done");
                        AnsiConsole.MarkupLine("Bye!");
                        return;
                    }
                    
                    //Prompt
                    Console.WriteLine();
                    int InputTokensBefore = MIRA.InputTokensConsumed;
                    int OutputTokensBefore = MIRA.OutputTokensConsumed;
                    string? cresponse = null;
                    try
                    {
                        cresponse = await MIRA.PromptAsync(input);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine("[red]Error during prompt: " + ex.Message + "[/]");
                    }

                    //Print
                    if (cresponse != null)
                    {
                        AnsiConsole.MarkupLine("[blue]" + Markup.Escape(cresponse) + "[/]");
                    }

                    //Print tokens consumption
                    int InputTokensConsumed = MIRA.InputTokensConsumed - InputTokensBefore;
                    int OutputTokensConsumed = MIRA.OutputTokensConsumed - OutputTokensBefore;
                    AnsiConsole.MarkupLine("[gray][italic]Input tokens: " + InputTokensConsumed.ToString("#,##0") + ", Output Tokens: " + OutputTokensConsumed.ToString("#,##0") + "[/][/]");
                    
                    //new lines
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            //Prompt it
            DateTimeOffset InferenceBegan = DateTimeOffset.Now;
            AnsiConsole.MarkupLine("NOW RUNNING AGENT!");
            string? response = null;
            try
            {
                string user_prompt = prompt.TradingPrompt().Trim();
                if (custom_instructions != null)
                {
                    user_prompt = user_prompt + "\n\nUser-Specified custom instructions to follow: " + custom_instructions;
                }
                response = await MIRA.PromptAsync(user_prompt);
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
            state.InputTokensConsumed = state.InputTokensConsumed + MIRA.InputTokensConsumed;
            state.OutputTokensConsumed = state.OutputTokensConsumed + MIRA.OutputTokensConsumed;
            
            //Stats
            TimeSpan AwakeFor = DateTimeOffset.UtcNow - WakeUpTime;
            TimeSpan InferenceTime = InferenceEnded - InferenceBegan;
            float TokensPerMinute = Convert.ToSingle(MIRA.InputTokensConsumed + MIRA.OutputTokensConsumed) / Convert.ToSingle(InferenceTime.TotalMinutes);
            AnsiConsole.MarkupLine("Ran for [bold]" + AwakeFor.TotalMinutes.ToString("#,##0.0") + " minutes[/], inference for [bold]" + InferenceTime.TotalMinutes.ToString("#,##0.0") + " minutes[/]: [bold]" + MIRA.InputTokensConsumed.ToString("#,##0") + "[/] input tokens, [bold]" + MIRA.OutputTokensConsumed.ToString("#,##0") + "[/] output tokens ([bold]" + TokensPerMinute.ToString("#,##0") + "[/] TPM)");

            //Save state!
            AnsiConsole.Markup("Saving state to storage... ");
            state.Save();
            AnsiConsole.MarkupLine("[green]saved![/]");

            //Shutdown
            AnsiConsole.MarkupLine("Going to sleep. Good bye.");
        }

        public static async Task ReviewPortfolioAsync()
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
            AnsiConsole.MarkupLine("[green]done after " + (pp_end_at - pp_start_at).TotalSeconds.ToString("#,##0.0") + " seconds[/]");
            Console.WriteLine();

            //Print
            AnsiConsole.MarkupLine("[bold][underline][blue]MIRA Managed Portfolio[/][/][/]");
            AnsiConsole.MarkupLine("Cash Balance: $" + state.Portfolio.Cash.ToString("#,##0.00"));
            AnsiConsole.MarkupLine("Trades made: " + state.Portfolio.HoldingTransactionLog.Count.ToString("#,##0"));
            Console.WriteLine();

            //Print total profitability
            AnsiConsole.MarkupLine("[underline]Profitability[/]");
            AnsiConsole.MarkupLine("Cash Invested: [bold]$" + pd.CashInjected.ToString("#,##0") + "[/]");
            AnsiConsole.MarkupLine("Total Portfolio Value (inc. cash): [bold]$" + (pd.HoldingsValue + pd.CashBalance).ToString("#,##0") + "[/]");
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

            //Create holdings table
            Table t = new Table();
            t.AddColumn("Symbol");
            t.AddColumn("Day Change");
            t.AddColumn("Price");
            t.AddColumn("Shares");
            t.AddColumn("Value");
            t.AddColumn("Allocation");
            t.AddColumn("Cost Basis");                
            t.AddColumn("Unr. Gain/Loss");
            t.AddColumn("Unr. Gain/Loss %");

            foreach (PortfolioHolding ph in sorted_phs)
            {
                //Calculate allocation %
                float allocationP = (ph.PositionValue / pd.HoldingsValue)*100;

                string color = ph.UnrealizedGainLoss >= 0 ? "green" : "red";
                string dayColor = ph.DayChangePercent >= 0 ? "green" : "red";

                string symbol = "[bold]" + ph.Symbol + "[/]";
                string dayChange = "[" + dayColor + "]" + ph.DayChangePercent.ToString("#0.0") + "%" + "[/]";
                string price = "$" + ph.CurrentPrice.ToString("#,##0.00");
                string quantity = ph.QuantityOwned.ToString("#,##0");
                string value = "$" + ph.PositionValue.ToString("#,##0");
                string allocation = allocationP.ToString("#0.0") + "%";
                string costBasis = "$" + ph.TotalCostBasis.ToString("#,##0");
                string gainLoss = "[" + color + "]" + "$" + ph.UnrealizedGainLoss.ToString("#,##0") + "[/]";
                string gainLossPercent = "[" + color + "]" + ph.UnrealizedGainLossPercent.ToString("#0.0") + "%" + "[/]";

                t.AddRow(symbol, dayChange, price, quantity, value, allocation, costBasis, gainLoss, gainLossPercent);
            }

            //Print table
            AnsiConsole.MarkupLine("[underline]Holdings[/]");
            AnsiConsole.Write(t);
            Console.WriteLine();

            //Print consumption metrics
            AnsiConsole.MarkupLine("[underline][gray]Cumulative Consumption[/][/]");
            AnsiConsole.MarkupLine("[gray]Input Tokens: " + state.InputTokensConsumed.ToString("#,##0") + "[/]");
            AnsiConsole.MarkupLine("[gray]Output Tokens: " + state.OutputTokensConsumed.ToString("#,##0") + "[/]");

            // Collect all unique dates with activity (trades or journal entries)
            HashSet<DateTime> activityDates = new HashSet<DateTime>();
            foreach (HoldingTransaction ht in state.Portfolio.HoldingTransactionLog)
            {
                activityDates.Add(ht.TransactedAt.LocalDateTime.Date);
            }
            foreach (JournalEntry je in state.InvestmentJournal)
            {
                activityDates.Add(je.EnteredAt.LocalDateTime.Date);
            }

            if (activityDates.Count == 0)
            {
                Console.WriteLine();
                AnsiConsole.MarkupLine("[gray]No activity history to review.[/]");
                return;
            }

            // Sort dates descending (most recent first)
            List<DateTime> sortedDates = activityDates.OrderByDescending(d => d).ToList();

            // Day-by-day review loop
            while (true)
            {
                Console.WriteLine();
                SelectionPrompt<string> daySelect = new SelectionPrompt<string>();
                daySelect.Title = "[underline]Activity History[/] - Select a day to review:";
                daySelect.AddChoice("← Back");
                foreach (DateTime d in sortedDates)
                {
                    daySelect.AddChoice(d.ToString("yyyy-MM-dd (dddd)"));
                }
                string daySelection = AnsiConsole.Prompt(daySelect);

                if (daySelection == "← Back")
                {
                    break;
                }

                // Parse selected date
                DateTime selectedDate = DateTime.ParseExact(daySelection.Substring(0, 10), "yyyy-MM-dd", null);
                Console.WriteLine();
                AnsiConsole.MarkupLine("[bold][underline][blue]" + selectedDate.ToString("MMMM d, yyyy (dddd)") + "[/][/][/]");

                // Trades for this day
                HoldingTransaction[] dayTrades = state.Portfolio.HoldingTransactionLog
                    .Where(ht => ht.TransactedAt.LocalDateTime.Date == selectedDate)
                    .OrderBy(ht => ht.TransactedAt)
                    .ToArray();

                if (dayTrades.Length > 0)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[underline]Trades[/]");
                    Table tradesTable = new Table();
                    tradesTable.AddColumn("Time");
                    tradesTable.AddColumn("Action");
                    tradesTable.AddColumn("Symbol");
                    tradesTable.AddColumn("Shares");
                    tradesTable.AddColumn("Price");
                    tradesTable.AddColumn("Total");

                    foreach (HoldingTransaction ht in dayTrades)
                    {
                        string time = ht.TransactedAt.LocalDateTime.ToString("h:mm tt");
                        string action = ht.OrderType == TimHanewich.Investing.Simulation.TransactionType.Buy ? "[green]Buy[/]" : "[red]Sell[/]";
                        string sym = "[bold]" + ht.Symbol + "[/]";
                        string shares = ht.Quantity.ToString("#,##0");
                        string px = "$" + ht.ExecutedPrice.ToString("#,##0.00");
                        string total = "$" + (ht.ExecutedPrice * ht.Quantity).ToString("#,##0.00");
                        tradesTable.AddRow(time, action, sym, shares, px, total);
                    }

                    AnsiConsole.Write(tradesTable);
                }
                else
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[gray]No trades on this day.[/]");
                }

                // Journal entries for this day
                JournalEntry[] dayJournals = state.InvestmentJournal
                    .Where(je => je.EnteredAt.LocalDateTime.Date == selectedDate)
                    .OrderBy(je => je.EnteredAt)
                    .ToArray();

                if (dayJournals.Length > 0)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[underline]Journal[/]");
                    foreach (JournalEntry je in dayJournals)
                    {
                        AnsiConsole.MarkupLine("[gray]" + je.EnteredAt.LocalDateTime.ToString("h:mm tt") + "[/]");
                        AnsiConsole.Write(new Panel(Markup.Escape(je.Entry)).BorderColor(Color.Grey));
                    }
                }
                else
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine("[gray]No journal entries on this day.[/]");
                }
            }
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