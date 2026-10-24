using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Translations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests.Controllers
{
    [TestClass]
    public class LanguageControllerTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Touch Localizations while HttpContext.Current is null: the static constructor then skips .po loading
            HttpContext.Current = null;
            if (!Internationalization.Localizations.ContainsKey("da"))
                Internationalization.Localizations["da"] = new Localization();
        }

        [TestCleanup]
        public void Cleanup()
        {
            HttpContext.Current = null;
        }

        private static LanguageController CreateControllerWithHttpContext(bool secureConnection = false)
        {
            HttpContext.Current = secureConnection
                ? new HttpContext(new SecureWorkerRequest())
                : new HttpContext(new HttpRequest("", "http://localhost/", ""), new HttpResponse(new StringWriter()));
            var wrapper = new HttpContextWrapper(HttpContext.Current);
            var controller = new LanguageController();
            controller.ControllerContext = new ControllerContext(wrapper, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(wrapper, new RouteData()));
            return controller;
        }

        /// <summary>A request that reports an HTTPS connection (HttpRequest built from a URL never does).</summary>
        private sealed class SecureWorkerRequest : SimpleWorkerRequest
        {
            public SecureWorkerRequest() : base("/", Path.GetTempPath(), "", "", new StringWriter())
            {
            }

            public override bool IsSecure()
            {
                return true;
            }
        }

        private static HttpCookie LanguageCookie()
        {
            var cookies = HttpContext.Current.Response.Cookies;
            Assert.IsTrue(cookies.AllKeys.Contains(Internationalization.Settings.LanguageCodeCookieKey), "Language cookie was not set");
            return cookies[Internationalization.Settings.LanguageCodeCookieKey];
        }

        private static void AssertHardened(HttpCookie cookie)
        {
            Assert.IsTrue(cookie.HttpOnly, "HttpOnly");
            Assert.AreEqual(SameSiteMode.Lax, cookie.SameSite, "SameSite");
            var expected = DateTime.Now.AddMonths(12);
            Assert.IsTrue(Math.Abs((cookie.Expires - expected).TotalDays) < 1, $"Expected about 12 months, got {cookie.Expires:o}");
        }

        [TestMethod]
        public void Set_SupportedCode_CookieHardened_OverHttp_NotSecure()
        {
            CreateControllerWithHttpContext().Set("da", "/");

            var cookie = LanguageCookie();
            AssertHardened(cookie);
            Assert.IsFalse(cookie.Secure, "Secure must follow the connection, so local http still works");
        }

        [TestMethod]
        public void Set_SupportedCode_CookieHardened_OverHttps_Secure()
        {
            CreateControllerWithHttpContext(secureConnection: true).Set("da", "/");

            var cookie = LanguageCookie();
            AssertHardened(cookie);
            Assert.IsTrue(cookie.Secure, "Secure");
        }

        private static void AssertBadRequest(ActionResult result)
        {
            var status = result as HttpStatusCodeResult;
            Assert.IsNotNull(status, "Expected a bare status code result, not a view");
            Assert.AreEqual(400, status.StatusCode);
        }

        [TestMethod]
        public void Set_MissingCode_Returns400_NoException()
        {
            AssertBadRequest(new LanguageController().Set(null, "/"));
        }

        [TestMethod]
        public void Set_BlankCode_Returns400()
        {
            AssertBadRequest(new LanguageController().Set("  ", "/"));
        }

        [TestMethod]
        public void Set_UnsupportedCode_Returns400()
        {
            AssertBadRequest(new LanguageController().Set("xx", "/"));
        }

        [TestMethod]
        public void IsSupported_NullOrBlank_IsFalse()
        {
            Assert.IsFalse(Internationalization.IsSupported(null));
            Assert.IsFalse(Internationalization.IsSupported(""));
        }

        [TestMethod]
        public void Set_SupportedCode_LocalReturnUrl_RedirectsToReturnUrl()
        {
            var controller = CreateControllerWithHttpContext();

            var result = controller.Set("da", "/Flight/Grid") as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("/Flight/Grid", result.Url);
            var cookies = HttpContext.Current.Response.Cookies;
            Assert.IsTrue(cookies.AllKeys.Contains(Internationalization.Settings.LanguageCodeCookieKey));
            Assert.AreEqual("da", cookies[Internationalization.Settings.LanguageCodeCookieKey].Value);
        }

        [TestMethod]
        public void Set_SupportedCode_ExternalReturnUrl_RedirectsToRoot()
        {
            var controller = CreateControllerWithHttpContext();

            var result = controller.Set("DA", "https://evil.example/") as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("/", result.Url);
        }

        [TestMethod]
        public void Set_SupportedCode_MissingReturnUrl_RedirectsToRoot()
        {
            var controller = CreateControllerWithHttpContext();

            var result = controller.Set("da", null) as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("/", result.Url);
        }
    }
}
