using System;
using System.Diagnostics;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{
    /// <summary>
    /// Maps Agile.Diagnostics.LogLevels to System.Diagnostics.Trace logLevels and writes to the Trace
    /// </summary>
    public class TraceLogger : ILogger
    {

        /// <summary>
        /// Output the text to the console.
        /// </summary>
        public void Write(string message, LogLevel level, LogCategory category, Type exType = null, int threadId = 0)
        {
            var standardMessage = Logger.GetStandardFormatMessage(message, level, category, threadId);
            switch (level)
            {
                case LogLevel.Error:
                    Trace.TraceError(standardMessage);
                    break;
                case LogLevel.Fatal:
                    Trace.TraceError(standardMessage);
                    break;
                case LogLevel.Warning:
                    Trace.TraceWarning(standardMessage);
                    break;
                default:
                    Trace.TraceInformation(standardMessage);
                    break;
            }
            
        }
    }
}