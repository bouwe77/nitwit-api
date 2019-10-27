using System;
using System.IO;

namespace nitwitapi
{
    public class Logger
    {
        public void Log(string message)
        {
            File.AppendAllText($"{Path.Combine(Constants.ApplicationFolder,"mylog.txt")}", $"[{DateTime.UtcNow}] {message}{Environment.NewLine}");
        }
    }
}
