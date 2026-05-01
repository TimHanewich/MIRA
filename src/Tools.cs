using System;

namespace AIA
{
    public class Tools
    {
        public static string ConfigDirectoryPath
        {
            get
            {
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIA");
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

            // 5. Convert back to DateTimeOffset to get the correct UTC offset for that date
            // This handles the switch between EST and EDT automatically
            DateTimeOffset result = TimeZoneInfo.ConvertTime(new DateTimeOffset(next3Pm), easternZone);

            return result;
        }

    }
}