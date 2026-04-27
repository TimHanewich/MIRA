using System;
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;
using TimHanewich.AgentFramework;
using Spectre.Console;

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

                //Wait until the next start time is tomorrow, not today
                while (Tools.NextWakeTime() < DateTimeOffset.Now)
                {
                    AnsiConsole.MarkupLine("Waiting until past trigger time.");
                    await Task.Delay(1_000);   
                }
            }
        }

        public static async Task WakeAsync()
        {
            
        }
    }
}