using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FlightJournal.Web.Extensions;

namespace FlightJournal.Tests.Extensions
{
    /// <summary>
    /// Validates that the TimeSpanExtensions function as expected
    /// </summary>
    [TestClass]
    public class TimeSpanExtensionTest
    {
        [TestMethod]
        public void FormattingBelowFullDayValues()
        {
            var time = new TimeSpan(0,10,20,0,0);
            string formatted = time.TotalHoursWithMinutesAsDecimal();
            Assert.AreEqual(formatted, "10:20");
        }

        [TestMethod]
        public void FormattingSmallValues()
        {
            var time = new TimeSpan(0, 26, 20, 0, 0);
            string formatted = time.TotalHoursWithMinutesAsDecimal();
            Assert.AreEqual(formatted, "26:20");
        }

        [TestMethod]
        public void FormattingAboveFullDayValues()
        {
            var time = new TimeSpan(2, 10, 20, 0, 0);
            string formatted = time.TotalHoursWithMinutesAsDecimal();
            Assert.AreEqual(formatted, "58:20");
        }
    }
}
