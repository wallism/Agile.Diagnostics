using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{
    /// <summary>
    /// Writes to logs to a file
    /// </summary>
    public class FileLogger : ILogger
    {
        /// <summary>
        /// ctor
        /// </summary>
        public FileLogger()
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="pathDirectory">Directory path where the file will be created.</param>
        public FileLogger(string pathDirectory)
        {
            PathDirectory = pathDirectory;
        }

        private string appName;
        /// <summary>
        /// Gets or sets the application name, defaults to assembly name if not set.
        /// </summary>
        public string AppName
        {
            get
            {
                if (string.IsNullOrEmpty(appName))
                {
                    try
                    {
                        var entryAssembly = Assembly.GetEntryAssembly();

                        appName = entryAssembly == null 
                            ? "AgileApp"
                            : entryAssembly.GetName().Name;
                    }
                    catch
                    {
                        appName = "appEx";
                    }
                }
                return appName; 
            }
            set
            {
                fullPath = null;
                appName = value; 
            }
        }

        private string pathDirectory;
        /// <summary>
        /// Gets or set the directory of the log file, defaults to AppDomain.CurrentDomain.BaseDirectory if not set
        /// </summary>
        public string PathDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(pathDirectory))
                    pathDirectory = AppDomain.CurrentDomain.BaseDirectory; 
                return pathDirectory; 
            }
            set
            {
                fullPath = null;
                pathDirectory = value; 
            }
        }

        private string fullPath;
        /// <summary>
        /// Gets or set the full file path of the log file, defaults to CurrentDirectory\AppName.log if not set
        /// </summary>
        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    // default to exeDir\appName.log 
                    fullPath = Path.Combine(PathDirectory, string.Format("{0}.log", AppName));
                }
                return fullPath;
            }
            set { fullPath = value; }
        }

        private int exceptionCount;


        private static object locker = new object();

        /// <summary>
        /// Output the text to the console.
        /// </summary>
        public void Write(string message, LogLevel level, LogCategory category, Type exType = null)
        {
            // stop trying to write if we've had more than 2 exceptions
            if (exceptionCount > 2)
                return;

            var standardMessage = Logger.GetStandardFormatMessage(message, level, category);
            // also append a new line to ensure log events always start on a new line
            standardMessage = string.Format("{0}\r\n", standardMessage);
            try
            {

                lock (locker)
                {
                    File.AppendAllText(FullPath, standardMessage);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // try to create it
                try
                {
                    Directory.CreateDirectory(PathDirectory);
                }
                catch (Exception ex)
                {// catch and do nothing if that failed
                    Logger.Error(ex);
                }
            }
            catch (Exception ex)
            {
                exceptionCount++;
                // catch and do nothing but write to Trace
                // dont ever let logging be a cause of an exception
                Trace.TraceError(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Returns true if the file exists and contains the given value.
        /// </summary>
        public bool FileContains(string value)
        {
            if (!File.Exists(FullPath))
                return false;
            var contents = File.ReadAllText(FullPath);
            if (string.IsNullOrEmpty(contents))
                return false;

            return contents.Contains(value);
        }

        public void ClearLog()
        {
            if (exceptionCount > 2)
                return;

            try
            {
                File.WriteAllText(FullPath, string.Empty);
            }
            catch (Exception ex)
            {
                // catch and do nothing but write to Trace
                // dont ever let logging be a cause of an exception
                Trace.TraceError(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }
    }
}