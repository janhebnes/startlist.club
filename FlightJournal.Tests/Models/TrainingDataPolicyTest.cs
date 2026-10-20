using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Models
{
    [TestClass]
    public class TrainingDataPolicyTest
    {
        // Synthetic pilots only; never use real user data in tests
        private static Pilot Instructor(int pilotId)
        {
            return new Pilot { PilotId = pilotId, InstructorId = "FI-TEST-" + pilotId };
        }

        private static Pilot PlainPilot(int pilotId)
        {
            return new Pilot { PilotId = pilotId };
        }

        [TestMethod]
        public void CanAccess_Instructor_True()
        {
            Assert.IsTrue(TrainingDataPolicy.CanAccess(true, false));
        }

        [TestMethod]
        public void CanAccess_Administrator_True()
        {
            Assert.IsTrue(TrainingDataPolicy.CanAccess(false, true));
        }

        [TestMethod]
        public void CanAccess_PlainPilot_False()
        {
            Assert.IsFalse(TrainingDataPolicy.CanAccess(false, false));
        }

        [TestMethod]
        public void ResolveInstructorId_BackseatInstructor_Wins()
        {
            Assert.AreEqual(11, TrainingDataPolicy.ResolveInstructorId(Instructor(11), Instructor(22)));
        }

        [TestMethod]
        public void ResolveInstructorId_CurrentUserInstructor_WhenBackseatNot()
        {
            Assert.AreEqual(22, TrainingDataPolicy.ResolveInstructorId(PlainPilot(11), Instructor(22)));
            Assert.AreEqual(22, TrainingDataPolicy.ResolveInstructorId(null, Instructor(22)));
        }

        [TestMethod]
        public void ResolveInstructorId_NoInstructor_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, TrainingDataPolicy.ResolveInstructorId(PlainPilot(11), PlainPilot(22)));
            Assert.AreEqual(-1, TrainingDataPolicy.ResolveInstructorId(null, null));
        }
    }
}
