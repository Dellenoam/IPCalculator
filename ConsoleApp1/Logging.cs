using System;
using System.IO;

namespace ConsoleApp1
{
    internal class Logging
    {
        public void logs(string text)
        {
            string path_logs = $"{Directory.GetCurrentDirectory()}\\logs.txt";

            StreamWriter sw = new StreamWriter(path_logs, true);
            sw.Write(text);
            sw.Close();
        }
    }
}
