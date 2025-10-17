using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;

using System.Linq;
using System.Text;



namespace FlightJournal.Web.App_Start
{
    /// <summary>
    /// Allowing sql command logging on exceptions received within entity framework db calls
    /// </summary>
    /// <seealso cref="System.Data.Entity.Database.Log"/>
    /// <see cref="https://docs.microsoft.com/en-us/ef/ef6/fundamentals/logging-and-interception"/>
    public class DbCommandInterceptor : IDbCommandInterceptor
    {
        private ConcurrentDictionary<DbCommand, DateTime> commandStartTimes = new();

        public void NonQueryExecuting(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogStartOfExec(command, interceptionContext);
        }

        public void NonQueryExecuted(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogEndOfExec(command, interceptionContext);
        }

        public void ReaderExecuting(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogStartOfExec(command, interceptionContext);
        }

        public void ReaderExecuted(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogEndOfExec(command, interceptionContext);
        }

        public void ScalarExecuting(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogStartOfExec(command, interceptionContext);
        }

        public void ScalarExecuted(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogEndOfExec(command, interceptionContext);
        }

        private void LogStartOfExec<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
#if (DEBUG)
            commandStartTimes.TryAdd(command, DateTime.Now);
#endif
        }

        private void LogEndOfExec<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
#if (DEBUG)
            var duration = (commandStartTimes.TryRemove(command, out var startTime))
                ? (int)(DateTime.Now - startTime).TotalMilliseconds
                : 0;
        
            System.Diagnostics.Trace.WriteLine($"-------{DateTime.Now:HH:mm:ss.fff}{Environment.NewLine}SQL Command executed {(interceptionContext.IsAsync ? "a" : "")}synchronously in {duration}ms: {Environment.NewLine}{OurStackEntries}{Environment.NewLine}SQL Command:{Environment.NewLine}{command.CommandText}{Environment.NewLine}Parameters: {Environment.NewLine}{CommandParametersToString(command.Parameters)}{Environment.NewLine}");
#endif
            if (interceptionContext.Exception != null)
            {
                // Check for the specific error about 'CreatedOn' in '__MigrationHistory' that is a check done by EF between 4.3 and 5 on init of databases
                if (command.CommandText.Contains("SELECT TOP (1) \r\n    [c].[CreatedOn] AS [CreatedOn]\r\n    FROM [dbo].[__MigrationHistory] AS [c]"))
                {
                    // Ignore this specific exception
                    // EF __Migration exception with CreatedOn is related to https://stackoverflow.com/a/20670134 and is being throwned often but captured 
                }
                else
                {
                    System.Diagnostics.Trace.TraceError($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
#if (DEBUG)
                    throw new Exception($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
#endif
                }

            }
            else
            {
#if (DEBUG)
                System.Diagnostics.Trace.WriteLine($"SQL Command completed successfully.{Environment.NewLine}-------");
#endif
            }
        }


        /// <summary>
        /// Resolve the command parameters to a formatted string 
        /// </summary>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        private static string CommandParametersToString(DbParameterCollection commandParameters)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                commandParameters?.Cast<DbParameter>()
                    .ToList()
                    .ForEach(p => sb.Append($"{p.ParameterName} = {p.Value}{Environment.NewLine}"));
            }
            catch (Exception e)
            {
                sb.Append($"Exception formatting parameters: {e.Message}");
            }

            return sb.ToString();
        }


        private string OurStackEntries =>
            string.Join(Environment.NewLine, Environment.StackTrace
            .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
            .Where(s => s.Contains("FlightJournal.Web") && !s.Contains(".DbCommandInterceptor"))
            );
    }
}