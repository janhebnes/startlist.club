using System;
using FlightJournal.Web.Models.Training;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    /// <summary>
    /// The bound shared by the training history page and its CSV/JSON exports.
    /// The controller actions cannot run in unit tests (static request state), so the shared clamp is tested here.
    /// </summary>
    [TestClass]
    public class TrainingHistoryRangeTest
    {
        private static readonly DateTime To = new DateTime(2001, 6, 30);

        [TestMethod]
        public void RangeOver366Days_IsClamped_EndingOnSelectedEndDate()
        {
            var clamped = TrainingHistoryRange.Clamp(To.AddYears(-3), To, out var from);

            Assert.IsTrue(clamped);
            Assert.AreEqual(To.AddDays(-366), from);
            Assert.AreEqual(366, (To - from).TotalDays);
        }

        [TestMethod]
        public void RangeOfExactly366Days_IsUnchanged()
        {
            var requestedFrom = To.AddDays(-366);

            var clamped = TrainingHistoryRange.Clamp(requestedFrom, To, out var from);

            Assert.IsFalse(clamped);
            Assert.AreEqual(requestedFrom, from);
        }

        [TestMethod]
        public void ShortRange_IsUnchanged()
        {
            var requestedFrom = new DateTime(2001, 6, 1);

            var clamped = TrainingHistoryRange.Clamp(requestedFrom, To, out var from);

            Assert.IsFalse(clamped);
            Assert.AreEqual(requestedFrom, from);
        }

        [TestMethod]
        public void MaxDays_Is366()
        {
            Assert.AreEqual(366, TrainingHistoryRange.MaxDays);
        }
    }
}
