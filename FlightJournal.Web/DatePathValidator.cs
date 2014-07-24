using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace FlightJournal.Web
{
    public interface IDateRouteValidator
    {
        bool IsValid(string date);
    }

    public class DatePathValidator : IDateRouteValidator
    {
        public DateTime DayZ;
        public DateTime MonthZ;
        public DateTime YearZ;

        public void Parse(string date)
        {
            IsValid(date);
        }

        /// <summary>
        /// Allow formats 2014 or 2014-03 or 2014-04-22
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsValid(string date)
        {
            if (date.Length == 10 
                && DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DayZ))
            {
                return true;
            }
            if (date.Length == 7 
                && DateTime.TryParseExact(date, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out MonthZ))
            {
                return true;
            }
            if (date.Length == 4 
                && DateTime.TryParseExact(date, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out YearZ))
            {
                return true;
            }
            return false;
        }
    }
}