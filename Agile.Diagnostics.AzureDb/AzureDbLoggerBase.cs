using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.AzureDb
{
    public abstract class AzureDbLoggerBase : ILogger
    {
        /// <summary>
        /// must be initialized with Source (e.g. project name) and Host  (machine or instance name?)
        /// </summary>
        public AzureDbLoggerBase(string source, string host)
        {
            Source = source ?? "";
            Host = host ?? "";
            // host could be anything, db limit is 50chars
            if (Host.Length > 50)
                Host = Host.Substring(0, 50);
        }

        private string connectionString;
        protected string Source { get; set; }
        protected string Host { get; set; }

        private string ConnectionString
        {
            get
            {
                try
                {
                    return connectionString ?? (connectionString = ConfigurationManager.ConnectionStrings["LoggingConnString"].ConnectionString);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("[{0}] {1}", ex, MethodBase.GetCurrentMethod().Name));
                    return string.Empty;
                }
            }
        }

        protected abstract string GetStoredProcName();


        public void Write(string message, LogLevel level, LogCategory category, Type exType = null, int threadId = 0)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    if (category == null) // make sure not null
                        category = LogCategory.General;

                    var command = new SqlCommand(GetStoredProcName(), connection);
                    command.CommandType = CommandType.StoredProcedure;
                    InitializeParameters(message, level, category, command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[{0}] {1}", ex, MethodBase.GetCurrentMethod().Name));
            }
        }

        protected virtual void InitializeParameters(string message, LogLevel level, LogCategory category, SqlCommand command)
        {
            command.Parameters.Add(new SqlParameter("@Message", SqlDbType.VarChar, 4096)).Value = message;
            command.Parameters.Add(new SqlParameter("@Level", SqlDbType.VarChar, 16)).Value = level.ToString();
            command.Parameters.Add(new SqlParameter("@Category", SqlDbType.VarChar, 32)).Value = category.Name;
            command.Parameters.Add(new SqlParameter("@Source", SqlDbType.VarChar, 60)).Value = Source;
            command.Parameters.Add(new SqlParameter("@Host", SqlDbType.VarChar, 50)).Value = Host;
            command.Parameters.Add(new SqlParameter("@Type", SqlDbType.VarChar, 100)).Value = GetExTypeFromMessage(message);

        }

        private static string GetExTypeFromMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message) && message.StartsWith("["))
                {
                    var endIndex = message.IndexOf("]");
                    if (endIndex != -1)
                    {
                        var type = message.Substring(1, endIndex - 1);
                        return type;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[{0}] {1}", ex, MethodBase.GetCurrentMethod().Name));
            }
            return string.Empty;
        }

    }
}