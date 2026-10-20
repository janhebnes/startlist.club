using System;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace FlightJournal.Web.Filters
{
    /// <summary>
    /// Anti-forgery validation for JSON posts: compares the cookie token with a request header,
    /// because the stock ValidateAntiForgeryToken only reads Request.Form.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public const string HeaderName = "__RequestVerificationToken";

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var headerToken = request.Headers[HeaderName];
            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            if (string.IsNullOrEmpty(headerToken) || cookie == null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing anti-forgery token");
                return;
            }
            try
            {
                AntiForgery.Validate(cookie.Value, headerToken);
            }
            catch (HttpAntiForgeryException)
            {
                // Expected client state (stale page / other user); answer 400, do not log as a server fault
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid anti-forgery token");
            }
        }
    }
}
