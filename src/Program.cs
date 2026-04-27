using System;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;
using TimHanewich.AgentFramework;
using Spectre.Console;
using TimHanewich.Foundry;
using Newtonsoft.Json.Linq;

namespace AIA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EnterAsync().Wait();
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
            AnsiConsole.MarkupLine("[bold]WAKING UP AT " + DateTimeOffset.Now.ToString() + "[/]");

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

            //Confirm portfolio
            AnsiConsole.MarkupLine("Portfolio with " + State.Load().Portfolio.Holdings().Length.ToString("#,##0") + " holdings loaded.");

            //Gather current portfolio value and such
            AnsiConsole.Markup("Gathering portfolio performance details... ");
            PortflioPerformance pp = await State.Load().Portfolio.CalculatePerformanceAsync();
            AnsiConsole.MarkupLine("[green]done[/]");

            //Construct prompt
            string prompt = Prompts.ConstructPrompt(State.Load().Portfolio, pp, State.Load().InvestmentJournal.ToArray());
            
            //Create the agent
            FoundryResource fr = new FoundryResource(settings.FoundryEndpoint);
            fr.ApiKey = settings.FoundryApiKey;
            Agent AIA = new Agent(prompt);
            AIA.FoundryResource = fr;
            AIA.Model = settings.FoundryModel;
            AIA.ExecutableFunctionInvoked += OnFunctionInvoked;
            
            //Register tools
            AIA.Tools.Add(new Quote());
            AIA.Tools.Add(new Buy());
            AIA.Tools.Add(new Sell());

            //Prompt it
            AnsiConsole.MarkupLine("NOW RUNNING AGENT!");
            string response = await AIA.PromptAsync("Please proceed with your goal. Go!");
            
            //Print response
            Console.WriteLine();
            AnsiConsole.MarkupLine("[blue]" + Markup.Escape(response) + "[/]");
        }



        //Function invoked
        public static void OnFunctionInvoked(ExecutableFunction ef, JObject args)
        {
            AnsiConsole.MarkupLine("Calling '" + ef.Name + "' with " + args.ToString(Newtonsoft.Json.Formatting.None) + "... ");
        }


    }
}