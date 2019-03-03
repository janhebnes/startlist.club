using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Extensions
{
    public static partial class HtmlHelperExtension
    {
        public static MvcHtmlString MenuLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues = null, object htmlAttributes = null)
        {
            var currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            var currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

            var builder = new TagBuilder("li")
            {
                InnerHtml = htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes).ToHtmlString()
            };

            if (controllerName == currentController && actionName == currentAction)
                builder.AddCssClass("active");

            return new MvcHtmlString(builder.ToString());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes, string iconAttributes)
        {
            var actionLink = htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            return MvcHtmlString.Create(actionLink.ToString().Replace(">" + linkText + "<", "><span class=\"" + iconAttributes + "\"></span> " + linkText + "<"));
        }

        /// <summary>
        /// Help selecting the active language translation
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="en">Default text identifier</param>
        /// <param name="da"></param>
        /// <returns></returns>
        public static string DisplayUserLanguage(this HtmlHelper htmlHelper, string en, string da = "")
        {
            if (Internationalization.LanguageCode == "da")
            {
                return da;
            }
            return Internationalization.GetText(en, Internationalization.LanguageCode);
        }
        
    }
}
