using System;
using System.IO;

namespace HINF
{
    public static class Log
    {
        public static void LogText(string path, string content, DateTime now)
        {
            File.AppendAllLines(path, new string[] { $"{now} :: {content}" });
        }

        public static string[] LoadLog(string path)
        {
            try
            {
                return File.ReadAllLines(path);
            }
            catch(FileNotFoundException)
            {
                return new string[1] { String.Empty };
            }
        }
    }
}
