using System.Web.Mvc;
using FlightJournal.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.AreEqual("Modify this template to jump-start your ASP.NET MVC application.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Administration()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Administration() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.UHB530() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
