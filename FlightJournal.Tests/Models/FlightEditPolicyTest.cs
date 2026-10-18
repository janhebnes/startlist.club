using System;
using FlightJournal.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class FlightEditPolicyTest
    {
        // Flight.Date is stored as a midnight date
        private static readonly DateTime FlightDate = new DateTime(2026, 10, 18);

        [TestMethod]
        public void Pilot_CanEdit_FlightDatedToday()
        {
            var now = FlightDate.AddHours(12);

            Assert.IsTrue(FlightEditPolicy.CanEdit(FlightDate, false, now));
        }

        [TestMethod]
        public void Pilot_CanEdit_FlightDatedTwoDaysAgo_BeforeMidnight()
        {
            var now = FlightDate.AddDays(2).AddHours(23);

            Assert.IsTrue(FlightEditPolicy.CanEdit(FlightDate, false, now));
        }

        [TestMethod]
        public void Pilot_CannotEdit_FlightOlderThanWindow()
        {
            var now = FlightDate.AddDays(3).AddMinutes(1);

            Assert.IsFalse(FlightEditPolicy.CanEdit(FlightDate, false, now));
        }

        [TestMethod]
        public void Editor_CanEdit_OldFlight()
        {
            var now = FlightDate.AddYears(1);

            Assert.IsTrue(FlightEditPolicy.CanEdit(FlightDate, true, now));
        }

        [TestMethod]
        public void DeniedMessage_HasNoFormatPlaceholders()
        {
            // A placeholder-free message can never be formatted with identity data
            Assert.IsFalse(FlightEditPolicy.DeniedMessage.Contains("{"));
        }
    }
}
