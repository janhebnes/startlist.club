using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FakeItEasy;
using FlightJournal.Web.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Filters
{
    [TestClass]
    public class ValidateJsonAntiForgeryTokenAttributeTest
    {
        private static AuthorizationContext CreateContext(NameValueCollection headers, HttpCookieCollection cookies)
        {
            var request = A.Fake<HttpRequestBase>();
            A.CallTo(() => request.Headers).Returns(headers);
            A.CallTo(() => request.Cookies).Returns(cookies);
            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.Request).Returns(request);
            return new AuthorizationContext { HttpContext = httpContext };
        }

        private static void AssertBadRequest(AuthorizationContext context)
        {
            var result = context.Result as HttpStatusCodeResult;
            Assert.IsNotNull(result, "The filter must short-circuit with a status code result");
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void MissingHeader_Returns400_NoException()
        {
            // Arrange
            var context = CreateContext(new NameValueCollection(), new HttpCookieCollection());

            // Act
            new ValidateJsonAntiForgeryTokenAttribute().OnAuthorization(context);

            // Assert
            AssertBadRequest(context);
        }

        [TestMethod]
        public void MissingCookie_Returns400_NoException()
        {
            // Arrange
            var headers = new NameValueCollection
            {
                { ValidateJsonAntiForgeryTokenAttribute.HeaderName, "synthetic-header-token" }
            };
            var context = CreateContext(headers, new HttpCookieCollection());

            // Act
            new ValidateJsonAntiForgeryTokenAttribute().OnAuthorization(context);

            // Assert
            AssertBadRequest(context);
        }
    }
}
