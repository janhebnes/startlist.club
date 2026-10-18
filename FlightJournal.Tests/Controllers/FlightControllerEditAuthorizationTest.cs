using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FakeItEasy;
using FlightJournal.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Controllers
{
    [TestClass]
    public class FlightControllerEditAuthorizationTest
    {
        // Synthetic identity only; never use real user data in tests
        private const string ProbeIdentity = "probe.user@example.invalid";

        private static FlightController CreateControllerForProbeUser()
        {
            var controller = new FlightController();
            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.User)
                .Returns(new GenericPrincipal(new GenericIdentity(ProbeIdentity), new string[0]));
            controller.ControllerContext = new ControllerContext(httpContext, new RouteData(), controller);
            return controller;
        }

        [TestMethod]
        public void EditDenied_ReturnsRestrictedRedirect_NotException()
        {
            // Arrange
            var controller = CreateControllerForProbeUser();

            // Act
            var result = controller.EditDenied("Edit") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.RouteValues["controller"]);
            Assert.AreEqual("Restricted", result.RouteValues["action"]);
        }

        [TestMethod]
        public void EditDenied_OutputContainsNoIdentity()
        {
            // Arrange
            var controller = CreateControllerForProbeUser();

            // Act
            var result = controller.EditDenied("Edit") as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            foreach (var value in result.RouteValues.Values)
            {
                var text = value == null ? string.Empty : value.ToString();
                Assert.IsFalse(text.Contains("probe.user"), "Route value leaks the user identity");
                Assert.IsFalse(text.Contains("@"), "Route value leaks an email-like identity");
            }
        }
    }
}
