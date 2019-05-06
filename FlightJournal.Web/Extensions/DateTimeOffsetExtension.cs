using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Extensions
{
    public static class DateTimeOffsetExtension
    {
        public enum RoundingDirection
        {
            Up,
            Down,
            Nearest
        }

        /// <summary>
        /// Rounds datetime up, down or to nearest minutes and all smaller units to zero
        /// </summary>
        /// <param name="dt">static extension method</param>
        /// <param name="rndmin">mins to round to</param>
        /// <param name="directn">Up,Down,Nearest</param>
        /// <returns>rounded datetime with all smaller units than mins rounded off</returns>
        /// <remarks>http://metadataconsulting.blogspot.com/2018/10/C-Round-Datetime-Extension-To-Nearest-Minute-And-Smaller-Units-Are-Rounded-To-Zero.html</remarks>
        public static DateTimeOffset RoundToNearestMinute(this DateTimeOffset dt, int rndmin = 1, RoundingDirection directn = RoundingDirection.Nearest)
        {
            if (rndmin == 0) //can be > 60 mins
                return dt;

            TimeSpan d = TimeSpan.FromMinutes(rndmin); //this can be passed as a parameter, or use any timespan unit FromDays, FromHours, etc.  

            long delta = 0;
            Int64 modTicks = dt.Ticks % d.Ticks;

            switch (directn)
            {
                case RoundingDirection.Up:
                    delta = modTicks != 0 ? d.Ticks - modTicks : 0;
                    break;
                case RoundingDirection.Down:
                    delta = -modTicks;
                    break;
                case RoundingDirection.Nearest:
                    {
                        bool roundUp = modTicks > (d.Ticks / 2);
                        var offset = roundUp ? d.Ticks : 0;
                        delta = offset - modTicks;
                        break;
                    }

            }
            return new DateTimeOffset(dt.Ticks + delta, dt.Offset);
        }
    }
}