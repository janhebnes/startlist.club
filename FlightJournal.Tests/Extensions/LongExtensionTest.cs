using System;
using System.Diagnostics;
using System.Globalization;
using FlightJournal.Web.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Extensions
{
    [TestClass]
    public class LongExtensionTest
    {
        [TestMethod]
        public void CatchingRoundBug()
        {
            // Summed ticks
            const long value = 71731720000; //Total

            // Specific flights
            const long flight1 = 5228130000;
            const long flight2 = 4842030000;
            const long flight3 = 58475460000;
            const long flight4 = 3186100000;
            
            // Values
            Debug.Print(value.TotalHoursWithMinutesAsDecimal());
            Debug.Print(flight1.TotalHoursWithMinutesAsDecimal());
            Debug.Print(flight2.TotalHoursWithMinutesAsDecimal());
            Debug.Print(flight3.TotalHoursWithMinutesAsDecimal());
            Debug.Print(flight4.TotalHoursWithMinutesAsDecimal());

            Debug.Print((flight1 + flight2 + flight3 + flight4).ToString());
            Debug.Print(value.ToString());

            // Analyse the output of a clean timespan
            var timeSpan = new TimeSpan(value);
            Debug.Print(timeSpan.ToString());
            Debug.Print(timeSpan.ToString("g"));
            Debug.Print(timeSpan.TotalHours.ToString(CultureInfo.InvariantCulture));
            Debug.Print(timeSpan.TotalHours.ToString("#00.0", CultureInfo.InvariantCulture) + " Please notice the rounding error that leads to an hour being added to the total");
            Debug.Print(timeSpan.TotalHours.ToString("#00.00", CultureInfo.InvariantCulture) + " Please notice the fixed rounded value");
            Debug.Print(timeSpan.TotalHours.ToString("#0.00", CultureInfo.InvariantCulture) + " Please notice the removed leading zero");
            Debug.Print(timeSpan.TotalMinutes.ToString(CultureInfo.InvariantCulture));
            Debug.Print(timeSpan.TotalMinutes.ToString("#00", CultureInfo.InvariantCulture));

            // Original formula
            var resultFormula = string.Format("{0}:{1}"
                    , timeSpan.TotalHours.ToString("#00.0").Substring(0, timeSpan.TotalHours.ToString("#00.0", CultureInfo.InvariantCulture).IndexOf(".", StringComparison.InvariantCulture))
                    , timeSpan.Minutes.ToString("#00"));
            Debug.Print(resultFormula);

            var resultFixedFormula = string.Format("{0}:{1}"
                , timeSpan.TotalHours.ToString("#0.000", CultureInfo.InvariantCulture).Substring(0, timeSpan.TotalHours.ToString("#0.000", CultureInfo.InvariantCulture).IndexOf(".", StringComparison.InvariantCulture))
                , timeSpan.Minutes.ToString("#00"));
            Debug.Print(resultFixedFormula);
            

            // Validate the resulting corrected formual is functioning
            var result = value.TotalHoursWithMinutesAsDecimal();
            Assert.AreEqual(result, "1:59");
        }
    }
}
