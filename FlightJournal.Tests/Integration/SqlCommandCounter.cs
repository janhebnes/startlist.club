using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Threading;

namespace FlightJournal.Tests.Integration
{
    /// <summary>
    /// Counts the SQL commands EF sends and keeps their text. Register with DbInterception.Add and remove it again in cleanup.
    /// </summary>
    public sealed class SqlCommandCounter : IDbCommandInterceptor
    {
        private int _count;
        public ConcurrentQueue<string> Commands { get; private set; } = new ConcurrentQueue<string>();
        public int Count => _count;

        public void Reset()
        {
            _count = 0;
            Commands = new ConcurrentQueue<string>();
        }

        private void Record(DbCommand command)
        {
            Interlocked.Increment(ref _count);
            Commands.Enqueue(command.CommandText);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) => Record(command);
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext) { }
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) => Record(command);
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext) { }
        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) => Record(command);
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext) { }
    }
}
