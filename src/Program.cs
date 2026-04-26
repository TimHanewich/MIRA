using System;

namespace AIA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AIASettings settings = AIASettings.Load();
            settings.FoundryEndpoint = "hi";
            settings.Save();
            Console.WriteLine(AIASettings.SavePath);
        }
    }
}