using System.Web.Mvc;
using FlightJournal.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Controllers
{
    [TestClass]
    public class AboutControllerTest
    {
        [TestMethod]
        public void Administration()
        {
            // Arrange
            AboutController controller = new AboutController();

            // Act
            ViewResult result = controller.Administration() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            AboutController controller = new AboutController();

            // Act
            ViewResult result = controller.UHB530() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
