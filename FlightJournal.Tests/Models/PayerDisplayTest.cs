using FlightJournal.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    /// <summary>The "(paid by ...)" rule: compare payer and pilot by id, never by object reference.</summary>
    [TestClass]
    public class PayerDisplayTest
    {
        private static Pilot NewPilot(int id) => new Pilot { PilotId = id, Name = "Test pilot " + id };

        [TestMethod]
        public void Flight_PayerIsPilot_AsDifferentInstancesWithSameId_IsNotSeparate()
        {
            var flight = new Flight { PilotId = 5, Pilot = NewPilot(5), BetalerId = 5, Betaler = NewPilot(5) };

            Assert.AreNotSame(flight.Pilot, flight.Betaler);
            Assert.IsFalse(flight.HasSeparatePayer());
        }

        [TestMethod]
        public void Flight_PayerIsAnotherPilot_IsSeparate()
        {
            var flight = new Flight { PilotId = 5, Pilot = NewPilot(5), BetalerId = 6, Betaler = NewPilot(6) };

            Assert.IsTrue(flight.HasSeparatePayer());
        }

        [TestMethod]
        public void Flight_NoPayer_IsNotSeparate()
        {
            var flight = new Flight { PilotId = 5, Pilot = NewPilot(5), BetalerId = 6, Betaler = null };

            Assert.IsFalse(flight.HasSeparatePayer());
        }

        [TestMethod]
        public void FlightVersionHistory_UsesTheSameRule()
        {
            Assert.IsFalse(new FlightVersionHistory { PilotId = 5, Pilot = NewPilot(5), BetalerId = 5, Betaler = NewPilot(5) }.HasSeparatePayer());
            Assert.IsTrue(new FlightVersionHistory { PilotId = 5, Pilot = NewPilot(5), BetalerId = 6, Betaler = NewPilot(6) }.HasSeparatePayer());
            Assert.IsFalse(new FlightVersionHistory { PilotId = 5, Pilot = NewPilot(5), BetalerId = 6, Betaler = null }.HasSeparatePayer());
        }
    }
}
