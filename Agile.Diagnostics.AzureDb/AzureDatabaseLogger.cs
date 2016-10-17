using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.AzureDb
{
    public class AzureDatabaseLogger : AzureDbLoggerBase
    {

        /// <summary>
        /// must be initialized with Source (e.g. AppConnect) and Host if it is available (machine or instance name?)
        /// </summary>
        public AzureDatabaseLogger(string source, string host)
            : base(source, host)
        {
        }


        protected override string GetStoredProcName()
        {
            return "LoggingInsert";
        }

        protected override void InitializeParameters(string message, LogLevel level, LogCategory category, SqlCommand command)
        {
            base.InitializeParameters(message, level, category, command);

            command.Parameters.Add(new SqlParameter("@ThreadId", SqlDbType.VarChar, 4)).Value = Logger.GetCurrentManagedThreadId().ToString();

            command.Parameters.Add(new SqlParameter("@LoggingId", SqlDbType.Int) { Direction = ParameterDirection.Output });
        }
    }
}