using System;

namespace FlightJournal.Web.Aprs
{
    public class FlightAnalysisWorkerOptions
    {
        /// <summary>Position updates waiting for analysis; further posts are dropped and counted.</summary>
        public int QueueCapacity { get; set; } = 20000;

        /// <summary>An aircraft not heard from for this long is detached from the engine.</summary>
        public TimeSpan ContextIdleExpiry { get; set; } = TimeSpan.FromHours(1);

        /// <summary>UTC hour of the daily engine recycle.</summary>
        public int RecycleHourUtc { get; set; } = 2;

        /// <summary>Index faults beyond this many recycles within an hour suspend processing.</summary>
        public int MaxFaultRecyclesPerHour { get; set; } = 6;

        public TimeSpan SuspendDuration { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>At most one exception is sent to telemetry per interval; the rest are counted.</summary>
        public TimeSpan ExceptionSampleInterval { get; set; } = TimeSpan.FromMinutes(5);
    }
}
