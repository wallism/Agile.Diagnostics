﻿using System;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{
    public class DebugLogger : ILogger
    {
        public void Write(string message, LogLevel level, LogCategory category, Type exType = null, int threadId = 0)
        {
            System.Diagnostics.Debug.WriteLine(Logger.GetStandardFormatMessage(message, level, category, threadId));
        }

    }
}
