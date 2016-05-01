using System;

namespace Agile.Diagnostics.Logging
{
    public interface ILogger 
    {
        /// <summary>
        /// write to the log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="exType"></param>
        /// <param name="threadId">needs to be passed in or gets the processors thread every time</param>
        void Write(string message, LogLevel level, LogCategory category, Type exType = null, int threadId = 0);
    }
}