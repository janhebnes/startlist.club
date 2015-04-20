using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJournal.Web.Extensions
{
    public static class TimeSpanExtension
    {
        /// <summary>
        /// Format the TimeSpan as TotalHours : Minutes 
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string TotalHoursWithMinutesAsDecimal(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes > 0)
            {
                return string.Format("{0}:{1}"
                    , timeSpan.TotalHours.ToString("#00.0").Substring(0, timeSpan.TotalHours.ToString("#00.0", CultureInfo.InvariantCulture).IndexOf(".", StringComparison.InvariantCulture))
                    , timeSpan.Minutes.ToString("#00"));
            }

            return string.Empty;
        }
    }
}
