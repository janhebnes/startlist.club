using System;
using Innovative.SolarCalculator;

namespace FlightJournal.Web.Aprs
{
    public class AprsActivityFilter
    {
        private bool blocked;

        public void Block(bool block)
        {
            blocked = block;
        }

        public bool ShouldAcceptFlight(DateTime now, Tuple<DateTime,DateTime> window, double latitude, double longitude)
        {
            if (blocked) return false; // override

            // within explicit time window ?
            if (now < window.Item1 ||
                now > window.Item2) return false;

            // within (position-dependent) sunlight window
            var solarTimes = new SolarTimes(now.Date, latitude, longitude);
            if (now < solarTimes.DawnCivil ||
                now > solarTimes.DuskCivil) return false;

            return true;

        }
    }
}