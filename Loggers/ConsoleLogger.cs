using System;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{
    public class ConsoleLogger : ILogger
    {
        public void Write(string message, LogLevel level, LogCategory category, Type exType = null)
        {
            Console.WriteLine(Logger.GetStandardFormatMessage(message, level, category));
        }

    }
}