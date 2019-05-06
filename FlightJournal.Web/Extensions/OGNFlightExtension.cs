using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Extensions
{
    public static class OGNFlightExtension
    {
        /// <summary>
        /// Rounded to nearest 1 minutes tkof_time adding the nearest 5 minutes flight_time for logbook
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public static DateTimeOffset? ldg_time_for_logbook(this OGN.FlightLog.Client.Models.Flight flight)
        {
            if (!flight.ldg_time.HasValue) return null;
            if (!flight.tkof_time.HasValue) return flight.ldg_time;

            var tkof = tkof_time_for_logbook(flight);
            var flighttime = flight_time_for_logbook(flight);

            if (!tkof.HasValue || !flighttime.HasValue)
            {
                return flight.ldg_time;
            }

            return tkof.Value.AddTicks(flighttime.Value.Ticks);
        }

        /// <summary>
        /// Rounded to nearest 5 minutes flight time
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public static TimeSpan? flight_time_for_logbook(this OGN.FlightLog.Client.Models.Flight flight)
        {
            if (!flight.ldg_time.HasValue) return null;
            if (!flight.tkof_time.HasValue) return null;

            TimeSpan actual_flight_time = flight.ldg_time.Value - flight.tkof_time.Value;
            return actual_flight_time.RoundToNearestMinute(5, TimeSpanExtension.RoundingDirection.Nearest);
        }

        /// <summary>
        /// Rounded to nearest 1 minute 
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public static DateTimeOffset? tkof_time_for_logbook(this OGN.FlightLog.Client.Models.Flight flight)
        {
            if (!flight.tkof_time.HasValue) return null;

            return flight.tkof_time.Value.RoundToNearestMinute(1, DateTimeOffsetExtension.RoundingDirection.Nearest);
        }
    }
}