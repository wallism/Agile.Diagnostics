using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Agile.Diagnostics.Loggers;
using System.Linq;

namespace Agile.Diagnostics.Logging
{
    public static class Logger
    {
        private static string dateFormat;
        public static string DateFormat 
        {
            get
            {
                if (string.IsNullOrEmpty(dateFormat))
                    dateFormat = string.Format("{0} {1}", DateOnlyFormat, TimeOnlyFormat);
                return dateFormat;
            }
        }

        public const string DateOnlyFormat = "yyyy-MMM-dd";
        public const string TimeOnlyFormat = "hh:mm:ss:fff";

        public static string GetStandardFormatMessage(string message, LogLevel level, LogCategory category)
        {
            var cat = (category != null) ? category.Abbreviation ?? string.Empty : string.Empty;
            var thread = GetCurrentManagedThreadId();

            return string.Format("{2} [{1}:{3}{4}] {0}",
                message ?? string.Empty,
                level.ToString().PadLeft(5, ' '), 
                GetDateString(DateTime.Now),
                cat, 
                string.IsNullOrEmpty(thread) ? "" : string.Format(" {0}", thread.PadLeft(3, ' '))); // can't add Thread Id because not availabel in Portable (well, can if really want, but in a version that is not part of a pcl.)
        }

        public static string GetDateString (DateTime date)
        {
            return date.ToString(DateFormat); 
        }

        public static readonly List<ILogger> AllLoggers = new List<ILogger>();

        /// <summary>
        /// Set this using Flags to explicitly include logging 'types' (not really 'levels' since we're using flags)
        /// By default 'All' logs are switched on.
        /// </summary>
        public static LogLevel IncludedLogs { get; set; }

        static Logger()
        {
            // Default loggers so logging works without any initializaton
            InitializeDefaults();
        }

        public static void InitializeDefaults()
        {
            InitializeLogging(new List<ILogger>
                                {
                                    new DebugLogger()
                                }, LogLevel.All);
        }

        private static void Write(string message, LogLevel level, LogCategory category, Type errorType, params object[] args)
        {
            if ((IncludedLogs & level) == level)
            {
                for (int i = 0; i < AllLoggers.Count; i++)
                {
                    try
                    {
                        var logger = AllLoggers[i];
                        logger.Write(string.Format(message, args), level, category, errorType);
                    }
                    catch (FormatException fx)
                    {
                        System.Diagnostics.Debug.WriteLine(message + " | FORMAT ERROR! (there's a {0} without an arg...or some text with )");
                        System.Diagnostics.Debug.WriteLine(fx.Message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR logging: {0}", ex.Message);
                    }
                }
            }
        }

        public static void InitializeLogging(ILogger logger, LogLevel includedLogLevels)
        {
            AllLoggers.Clear();
            AllLoggers.Add(logger);
            IncludedLogs = includedLogLevels;
        }

        public static bool HasInitializeBeenCalled { get; set; }

        public static void InitializeLogging(List<ILogger> loggers, LogLevel includedLogLevels, bool logCount = true)
        {
            HasInitializeBeenCalled = true;
            AllLoggers.Clear();
            AllLoggers.AddRange(loggers);
            IncludedLogs = includedLogLevels;
            if(logCount)
                Debug("{0} Loggers Initialized", loggers.Count);
        }

        public static void Debug(string message, params object[] args)
        {
            Write(message, LogLevel.Debug, LogCategory.General, null, args);
        }

        public static void Debug(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Debug, category, null, args);
        }

        public static void Debug(object message, params object[] args)
        {
            Write(message.ToString(), LogLevel.Debug, LogCategory.General, null, args);
        }

        public static void Debug(object message, LogCategory category, params object[] args)
        {
            Write(message.ToString(), LogLevel.Debug, category, null, args);
        }

        public static void Info(string message, params object[] args)
        {
            Write(message, LogLevel.Info, LogCategory.General, null, args);
        }

        public static void Info(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Info, category, null, args);
        }

        public static void Warning(string message, params object[] args)
        {
            Write(message, LogLevel.Warning, LogCategory.General, null, args);
        }

        public static void Warning(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Warning, category, null, args);
        }

        public static void Setup(string message, params object[] args)
        {
            Write(message, LogLevel.Info, LogCategory.Setup, null, args);
        }

        public static void Setup(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Info, category, null, args);
        }

        public static void Error(Exception ex)
        {
            Error(ex, string.Empty);
        }


        public static void Error(Exception ex, string extraMessage, params object[] args)
        {
            if (ex == null)
            {
                Write(string.Format("[noType][{0}] ex is null", extraMessage)
                , LogLevel.Error, LogCategory.Exception
                , null
                , args);
                return;
            }
            ProcessExtraExceptionActions(ex);

            var exTypeName = ex.GetType().Name;
            var message = ex.Message;
            var stack = string.IsNullOrEmpty(ex.StackTrace)
                ? ""
                : string.Format("\r\n{0}", ex.StackTrace);
            
            Write(string.Format("[{0}][{1}]{2}{3}", exTypeName, extraMessage, message, stack)
                , LogLevel.Error, LogCategory.Exception
                , ex.GetType()
                , args);
            // now log all inner exceptions
            if(ex.InnerException != null)
                LogInnerExceptions(ex.InnerException, 1);

        }

        public static void AddExceptionAction(Action<Exception> action)
        {
            if(!extraExceptionActions.Contains(action))
                extraExceptionActions.Add(action);
        }

        private static readonly List<Action<Exception>> extraExceptionActions = new List<Action<Exception>>(); 
        private static void ProcessExtraExceptionActions(Exception exception)
        {
            if(!extraExceptionActions.Any())
                return;

            foreach (var action in extraExceptionActions)
            {
                try
                {
                    action(exception);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private static void LogInnerExceptions(Exception ex, int count)
        {
            if (ex == null)
                return;

            var stack = string.IsNullOrEmpty(ex.StackTrace)
                ? ""
                : string.Format("\r\n{0}", ex.StackTrace);

            Write(string.Format(" [{0}]{1}{2}", ex.GetType().Name, ex.Message, stack)
                , LogLevel.Error, LogCategory.Exception, null);

            count++;
            LogInnerExceptions(ex.InnerException, count);
        }
        
        public static void Error(string message, params object[] args)
        {
            Write(message, LogLevel.Error, LogCategory.General, null, args);
        }

        public static void Error(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Error, category, null, args);
        }



        public static void Fatal(string message, params object[] args)
        {
            Write(message, LogLevel.Fatal, LogCategory.General, null, args);
        }

        public static void Fatal(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Fatal, category, null, args);
        }

        public static void Performance(string message, params object[] args)
        {
            Write(message, LogLevel.Performance, LogCategory.General, null, args);
        }

        public static void Performance(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Performance, category, null, args);
        }

        public static void Testing(string message, params object[] args)
        {
            Write(message, LogLevel.Testing, LogCategory.General, null, args);
        }

        public static void Testing(string message, LogCategory category, params object[] args)
        {
            Write(message, LogLevel.Testing, category, null, args);
        }

        public static void AddLogger(ILogger logger)
        {
            if(AllLoggers.All(existing => existing.GetType().Name != logger.GetType().Name))
                AllLoggers.Add(logger);
        }

        public static string GetCurrentManagedThreadId()
        {

#if MOBILE
            return string.Empty;
#else
            try
            {
                
                return Thread.CurrentThread.ManagedThreadId.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
#endif

        }
    }    
}