using System;
using System.Threading;

namespace FlightJournal.Web.Aprs
{
    public class TokenSleeper
    {
        private readonly CancellationToken _token;
        
        public TokenSleeper(CancellationToken token)
        {
            _token = token;
        }

        public bool Sleep(TimeSpan t)
        {
            if (t.Ticks <= 0) return true;
            var timeout = t.TotalMilliseconds < int.MaxValue ? (int)t.TotalMilliseconds : -1;
            return WaitHandle.WaitAny(new[] {_token.WaitHandle}, timeout) == WaitHandle.WaitTimeout;
        }
    }
}