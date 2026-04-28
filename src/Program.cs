using System;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;
using TimHanewich.AgentFramework;
using Spectre.Console;
using TimHanewich.Foundry;
using Newtonsoft.Json.Linq;
using TheMotleyFool.Transcripts;
using Newtonsoft.Json;

namespace AIA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //EnterAsync().Wait();
            WakeAsync().Wait();
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
            //Print awake
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
            PortflioPerformance pp = await state.Portfolio.CalculatePerformanceAsync();
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
            prompt.PortfolioPerformance = pp;
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
            
            //Register tools
            AIA.Tools.Add(new Quote());
            AIA.Tools.Add(new Buy(state));
            AIA.Tools.Add(new Sell(state));
            AIA.Tools.Add(new LogJournal(state));
            AIA.Tools.Add(new ReadJournal(state));
            AIA.Tools.Add(new ReadEarningsCallTranscript());

            //Prompt it
            AnsiConsole.MarkupLine("NOW RUNNING AGENT!");
            int TradeLimit = 10; //the limit number of trades this agent can make in this one turn
            string? response = null;
            try
            {
                response = await AIA.PromptAsync("Please proceed with your goal. You can make up to " + TradeLimit.ToString() + " trades. Go! And Good luck.\nNOTE: I WANT YOU TO READ AT LEAST 3 EARNINGS CALL TRANSCRIPTS. THIS IS IMPORTANT.");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Prompt failed! Msg: " + Markup.Escape(ex.Message) + "[/]");
            }
        
            //Print response
            if (response != null)
            {
                Console.WriteLine();
                AnsiConsole.MarkupLine("[blue]" + Markup.Escape(response) + "[/]");
                Console.WriteLine();
            }
            
            //Incrment token count
            state.InputTokensConsumed = state.InputTokensConsumed + AIA.InputTokensConsumed;
            state.OutputTokensConsumed = state.OutputTokensConsumed + AIA.OutputTokensConsumed;
            AnsiConsole.MarkupLine("Consumption: [bold]" + state.InputTokensConsumed.ToString("#,##0") + "[/] input tokens, [bold]" + state.OutputTokensConsumed.ToString("#,##0") + "[/] output tokens");

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

        public static void OnThinking()
        {
            AnsiConsole.MarkupLine("[gray][italic]thinking...[/][/]");
        }


    }
}