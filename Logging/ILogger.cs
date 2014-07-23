using System;

namespace Agile.Diagnostics.Logging
{
    public interface ILogger 
    {
        void Write(string message, LogLevel level, LogCategory category, Type exType = null);
    }
}