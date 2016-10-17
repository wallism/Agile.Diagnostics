using System.Data;
using System.Data.SqlClient;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.AzureDb
{
    /// <summary>
    /// Don't add as a logger, instantiate a new instance per write that is required
    /// to ensure source and host work fine.
    /// </summary>
    /// <remarks>Did it this way so can re-use AzureDatabaseLogger code, _may_ be better to cut and paste that code and separate.</remarks>
    public class AzureClientLogger : AzureDbLoggerBase
    {

        /// <summary>
        /// must be initialized with Source (e.g. AppConnect) and Host if it is available (machine or instance name?)
        /// </summary>
        public AzureClientLogger(Error error)
            : base("needsToBeSet", "SetFromClientCall")
        {
            Error = error;

            Source = error.App;
            Host = error.Machine;
            ThreadId = error.ThreadId;
        }

        private Error Error { get; set; }
        private string ThreadId { get; set; }


        protected override string GetStoredProcName()
        {
            return "LoggingClientInsert";
        }

        protected override void InitializeParameters(string message, LogLevel level, LogCategory category, SqlCommand command)
        {
            base.InitializeParameters(message, level, category, command);

            command.Parameters.Add(new SqlParameter("@AppKey", SqlDbType.Int)).Value = Error.AppKey;
            command.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar, 32)).Value = Error.Version;
            command.Parameters.Add(new SqlParameter("@Component", SqlDbType.VarChar, 64)).Value = Error.Component;

            if(! string.IsNullOrEmpty(ThreadId))
                command.Parameters.Add(new SqlParameter("@ThreadId", SqlDbType.VarChar, 4)).Value = ThreadId;

            command.Parameters.Add(new SqlParameter("@LoggingClientId", SqlDbType.Int) { Direction = ParameterDirection.Output });
        }
    }
}