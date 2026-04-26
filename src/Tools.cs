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
    }
}