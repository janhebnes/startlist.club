using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Controllers
{
    /// <summary>Reflection guards so a future edit can't silently drop the controller's protection.</summary>
    [TestClass]
    public class TrainingLogControllerSecurityTest
    {
        [TestMethod]
        public void Controller_HasClassLevelAuthorize()
        {
            Assert.IsTrue(typeof(TrainingLogController).IsDefined(typeof(AuthorizeAttribute), false));
        }

        [TestMethod]
        public void NoAction_AllowsAnonymous()
        {
            Assert.IsFalse(typeof(TrainingLogController).IsDefined(typeof(AllowAnonymousAttribute), true));

            var anonymousActions = typeof(TrainingLogController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.IsDefined(typeof(AllowAnonymousAttribute), true))
                .Select(m => m.Name)
                .ToList();
            Assert.AreEqual(0, anonymousActions.Count, string.Join(", ", anonymousActions));
        }

        [TestMethod]
        public void UpdateTrainingFlight_IsPostOnly_AndValidatesJsonAntiForgery()
        {
            var action = typeof(TrainingLogController).GetMethod("UpdateTrainingFlight");

            Assert.IsNotNull(action);
            Assert.IsTrue(action.IsDefined(typeof(HttpPostAttribute), true), "UpdateTrainingFlight must be POST only");
            Assert.IsTrue(action.IsDefined(typeof(ValidateJsonAntiForgeryTokenAttribute), true), "UpdateTrainingFlight must validate the anti-forgery token");
        }

        [TestMethod]
        public void FlightData_HasNoInstructorIdProperty()
        {
            // Model binding is case-insensitive, so check case-insensitively
            var property = typeof(FlightData).GetProperty("instructorId",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Assert.IsNull(property, "The instructor must be derived server-side, never bound from the client");
        }
    }
}
