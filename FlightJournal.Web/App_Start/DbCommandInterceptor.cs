using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Web;

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
            if (!interceptionContext.IsAsync)
            {
#if (DEBUG)
            System.Diagnostics.Trace.WriteLine("Non-async command used: {0}", command.CommandText);
#endif
            }
        }

        private void LogIfError<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
#if (DEBUG)
            System.Diagnostics.Trace.WriteLine($"SQL: {Environment.NewLine}{command.CommandText}{Environment.NewLine}Parameters: {Environment.NewLine}{CommandParametersToString(command.Parameters)}{Environment.NewLine}StackTrace: {Environment.NewLine}{Environment.StackTrace}{Environment.NewLine}");
#endif
            if (interceptionContext.Exception != null)
            {
                System.Diagnostics.Trace.TraceError($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
#if (DEBUG)
                throw new Exception($"Command {command.CommandText} failed with exception {interceptionContext.Exception}");
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