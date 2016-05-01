using System;
using System.Diagnostics;
using System.IO;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{
    public class DayOfWeekFileLogger : ILogger
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DayOfWeekFileLogger(bool clearAtStartOfDay = true)
        {
            ClearAtStartOfDay = clearAtStartOfDay;
        }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="pathDirectory">Directory path where the file will be created.</param>
        /// <param name="namePrefix">prefix for the file name</param>
        /// <param name="clearAtStartOfDay">clear the file contents at the start of the day (first time logged to for the day)</param>
        public DayOfWeekFileLogger(string pathDirectory, string namePrefix = "", bool clearAtStartOfDay = true)
            : this(clearAtStartOfDay)
        {
            PathDirectory = pathDirectory;
            NamePrefix = namePrefix;
        }

        private string PathDirectory { get; set; }
        private string NamePrefix { get; set; }
        private bool ClearAtStartOfDay { get;set;}

        private FileLogger fileLogger;
        private FileLogger FileLogger
        {
            get 
            {
                if(fileLogger == null)
                    fileLogger = new FileLogger(PathDirectory);
                return fileLogger; 
            }
        }

        private DateTime now;
        public void Write(string message, LogLevel level, LogCategory category, Type exType = null, int threadId = 0)
        {
            now = DateTime.Now;
            // every time we write we need to set the day of week as the App name. (no real need to include the exe name)
            FileLogger.AppName = string.Format("{0}{1}", NamePrefix, now.DayOfWeek);

            ClearLogWhenNeeded();
            // then just pass through the job of writing to the file to the file logger.
            FileLogger.Write(message, level, category, null, threadId);
        }

        /// <summary>
        /// used to ensure we only check if the file needs to be cleared only once per day/session
        /// </summary>
        private string CheckedClearFileDay { get; set; }

        private void ClearLogWhenNeeded()
        {
            if (!ClearAtStartOfDay) return;

            var dayOfWeek = now.DayOfWeek.ToString();
            if (dayOfWeek.Equals(CheckedClearFileDay))
                return;
            CheckedClearFileDay = dayOfWeek;

            // no need to check if needs clearing if it doesn't exist
            if (!File.Exists(FileLogger.FullPath))
                return;

            // to determine if this is the first time written to today
            // just look for a log entry with todays date, if not found we need to clear the file
            if (! FileLogger.FileContains(Logger.GetDateString(now).Substring(0, Logger.DateOnlyFormat.Length)))
                FileLogger.ClearLog();

        }
    }
}
