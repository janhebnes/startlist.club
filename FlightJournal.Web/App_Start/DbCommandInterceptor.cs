using System;
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
        public void NonQueryExecuting(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfNonAsync(command, interceptionContext);
        }

        public void NonQueryExecuted(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ReaderExecuting(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogIfNonAsync(command, interceptionContext);
        }

        public void ReaderExecuted(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ScalarExecuting(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogIfNonAsync(command, interceptionContext);
        }

        public void ScalarExecuted(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        private void LogIfNonAsync<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
#if (DEBUG)
            string[] stackTraceLines = Environment.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string compactStackTraceView = string.Join(Environment.NewLine, stackTraceLines.Where(d => d.Contains(":line") && !d.Contains(".DbCommandInterceptor")));
            if (compactStackTraceView == string.Empty) compactStackTraceView = Environment.StackTrace;
            if (!interceptionContext.IsAsync)
            {
                System.Diagnostics.Trace.WriteLine($"-------{Environment.NewLine}SQL Command executing asynchronously: {Environment.NewLine}{compactStackTraceView}{Environment.NewLine}");
            }
            else
            {
                System.Diagnostics.Trace.WriteLine($"-------{Environment.NewLine}SQL Command executing synchronously: {Environment.NewLine}{compactStackTraceView}{Environment.NewLine}");
            }
#endif
        }

        private void LogIfError<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
#if (DEBUG)
            string[] stackTraceLines = Environment.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string compactStackTraceView = string.Join(Environment.NewLine, stackTraceLines.Where(d => d.Contains(":line") && !d.Contains(".DbCommandInterceptor")));
            if (compactStackTraceView == string.Empty) compactStackTraceView = Environment.StackTrace;
            System.Diagnostics.Trace.WriteLine($"-------{Environment.NewLine}SQL Command executed: {Environment.NewLine}{compactStackTraceView}{Environment.NewLine}SQL Command:{Environment.NewLine}{command.CommandText}{Environment.NewLine}Parameters: {Environment.NewLine}{CommandParametersToString(command.Parameters)}{Environment.NewLine}");
#endif
            if (interceptionContext.Exception != null)
            {
                System.Diagnostics.Trace.TraceError($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
#if (DEBUG)
                throw new Exception($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
#endif
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
    }
}