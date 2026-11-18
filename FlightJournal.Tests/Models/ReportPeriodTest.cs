using System;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class ReportPeriodTest
    {
        [TestMethod]
        public void ForDay_IsOneDayFromMidnight()
        {
            var period = ReportPeriod.ForDay(new DateTime(2024, 2, 29, 13, 45, 0));

            Assert.AreEqual(ReportPeriodKind.Day, period.Kind);
            Assert.AreEqual(new DateTime(2024, 2, 29), period.Start);
            Assert.AreEqual(new DateTime(2024, 3, 1), period.EndExclusive);
        }

        [TestMethod]
        public void ForMonth_EndsAtTheFirstOfTheNextMonth()
        {
            var february = ReportPeriod.ForMonth(2024, 2);
            var december = ReportPeriod.ForMonth(2023, 12);

            Assert.AreEqual(ReportPeriodKind.Month, february.Kind);
            Assert.AreEqual(new DateTime(2024, 2, 1), february.Start);
            Assert.AreEqual(new DateTime(2024, 3, 1), february.EndExclusive);
            Assert.AreEqual(new DateTime(2024, 1, 1), december.EndExclusive);
        }

        [TestMethod]
        public void ForYear_EndsAtTheFirstOfJanuaryNextYear()
        {
            var period = ReportPeriod.ForYear(2023);

            Assert.AreEqual(ReportPeriodKind.Year, period.Kind);
            Assert.AreEqual(new DateTime(2023, 1, 1), period.Start);
            Assert.AreEqual(new DateTime(2024, 1, 1), period.EndExclusive);
        }

        [TestMethod]
        public void ClubFilter_FromClubWithoutShortName_IsUnfiltered()
        {
            Assert.IsTrue(ReportClubFilter.From(new Club()).IsUnfiltered);
            Assert.IsTrue(ReportClubFilter.From(null).IsUnfiltered);
            Assert.IsTrue(ReportClubFilter.Unfiltered.IsUnfiltered);

            var club = ReportClubFilter.From(new Club { ShortName = "TST", ClubId = 7, LocationId = 11 });
            Assert.IsFalse(club.IsUnfiltered);
            Assert.AreEqual(7, club.ClubId);
            Assert.AreEqual(11, club.LocationId);
        }
    }
}
