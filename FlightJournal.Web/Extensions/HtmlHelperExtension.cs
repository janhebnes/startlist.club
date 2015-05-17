using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using FlightJournal.Web.Controllers;

namespace FlightJournal.Web.Extensions
{
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString MenuLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            var currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            var currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

            var builder = new TagBuilder("li")
            {
                InnerHtml = htmlHelper.ActionLink(linkText, actionName, controllerName).ToHtmlString()
            };

            if (controllerName == currentController && actionName == currentAction)
                builder.AddCssClass("active");

            return new MvcHtmlString(builder.ToString());
        }

        /// <summary>
        /// Help selecting the active language translation
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="en"></param>
        /// <param name="da"></param>
        /// <returns></returns>
        public static string DisplayUserLanguage(this HtmlHelper htmlHelper, string en, string da = "")
        {
            if (LanguageController.UserLanguages.DefaultLanguage() == "da")
            {
                return da;
            }
            return en;
        }

        /// <summary>
        /// Help return the current language selected in the browser
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static string DefaultUserLanguage(this HtmlHelper htmlHelper)
        {
            return LanguageController.UserLanguages.DefaultLanguage();
        }
    }
}