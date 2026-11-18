using System;

namespace FlightJournal.Web.Aprs
{
    public class FlightAnalysisWorkerOptions
    {
        /// <summary>Position updates waiting for analysis; further posts are dropped and counted.</summary>
        public int QueueCapacity { get; set; } = 20000;

        public const int DefaultContextIdleExpiryHours = 6;

        /// <summary>
        /// An aircraft not heard from for this long is detached from the engine. Cross-country flights are out of
        /// receiver range for hours (the longest last 4-6 hours), and detaching earlier loses the flight context, so the
        /// landing on return isn't tied to its takeoff. The nightly recycle still clears all contexts once a day.
        /// </summary>
        public TimeSpan ContextIdleExpiry { get; set; } = TimeSpan.FromHours(DefaultContextIdleExpiryHours);

        /// <summary>
        /// The idle expiry from an app setting in whole hours: missing or unparsable gives the default of 6, and values
        /// are clamped to 1..24.
        /// </summary>
        public static TimeSpan ContextIdleExpiryFromSetting(string value)
        {
            var hours = int.TryParse(value, out var parsed) ? Math.Min(24, Math.Max(1, parsed)) : DefaultContextIdleExpiryHours;
            return TimeSpan.FromHours(hours);
        }

        /// <summary>UTC hour of the daily engine recycle.</summary>
        public int RecycleHourUtc { get; set; } = 2;

        /// <summary>Index faults beyond this many recycles within an hour suspend processing.</summary>
        public int MaxFaultRecyclesPerHour { get; set; } = 6;

        public TimeSpan SuspendDuration { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>At most one exception is sent to telemetry per interval; the rest are counted.</summary>
        public TimeSpan ExceptionSampleInterval { get; set; } = TimeSpan.FromMinutes(5);
    }
}
