using System;
using Spectre.Console;

namespace MIRA
{
    public class Tools
    {
        public static string ConfigDirectoryPath
        {
            get
            {
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MIRA");
                if (System.IO.Directory.Exists(path) == false)
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static DateTimeOffset NextWakeTime()
        {
            // 1. Get the Eastern Time zone
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            // 2. Get the current time in Eastern Time
            DateTime currentEasternTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, easternZone);

            // 3. Create 3 PM for today
            DateTime next3Pm = new DateTime(currentEasternTime.Year, currentEasternTime.Month, currentEasternTime.Day, 15, 0, 0);

            // 4. If 3 PM has already passed today, move to tomorrow
            if (next3Pm <= currentEasternTime)
            {
                next3Pm = next3Pm.AddDays(1);
            }

            // 5. Skip weekends (Saturday -> Monday, Sunday -> Monday)
            while (next3Pm.DayOfWeek == DayOfWeek.Saturday || next3Pm.DayOfWeek == DayOfWeek.Sunday)
            {
                next3Pm = next3Pm.AddDays(1);
            }

            // 5. Convert to DateTimeOffset using the correct Eastern Time UTC offset for that date
            // GetUtcOffset handles the switch between EST and EDT automatically
            TimeSpan easternOffset = easternZone.GetUtcOffset(next3Pm);
            DateTimeOffset result = new DateTimeOffset(next3Pm, easternOffset);

            return result;
        }

        public static string? AskForCustomInstructions()
        {
            TextPrompt<string> question = new TextPrompt<string>("Custom Instructions for MIRA: ");
            question.AllowEmpty = true;
            string CustomInstructions = AnsiConsole.Prompt(question);
            if (CustomInstructions == "")
            {
                return null;
            }
            else
            {
                return CustomInstructions;
            }
        }

    }
}