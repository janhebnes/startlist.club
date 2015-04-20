using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJournal.Web.Extensions
{
    public static class LongExtension
    {
        /// <summary>
        /// Format the TimeSpan Ticks as TotalHours : Minutes 
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static string TotalHoursWithMinutesAsDecimal(this long ticks)
        {
            return (new TimeSpan(ticks)).TotalHoursWithMinutesAsDecimal();
        }
    }
}
