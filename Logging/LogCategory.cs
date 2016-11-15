namespace Agile.Diagnostics.Logging
{
    public class LogCategory
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }

        private static LogCategory general;
        /// <summary>
        /// Gets the general category
        /// </summary>
        public static LogCategory General
        {
            get
            {
                if (general == null)
                    general = new LogCategory { Name = "General", Abbreviation = "GEN"};
                return general;
            }
        }

        private static LogCategory setup;
        /// <summary>
        /// Gets the Setup category
        /// </summary>
        public static LogCategory Setup
        {
            get
            {
                if (setup == null)
                    setup = new LogCategory { Name = "Setup", Abbreviation = "SETUP" };
                return setup;
            }
        }

        private static LogCategory exception;
        /// <summary>
        /// Gets the exception category
        /// </summary>
        public static LogCategory Exception
        {
            get
            {
                if (exception == null)
                    exception = new LogCategory { Name = "Exception", Abbreviation = "ERR" };
                return exception;
            }
        }

        private static LogCategory console;
        /// <summary>
        /// Gets the console category
        /// </summary>
        public static LogCategory Console
        {
            get
            {
                if (console == null)
                    console = new LogCategory { Name = "Console", Abbreviation = "CSL" };
                return console;
            }
        }
    }
}