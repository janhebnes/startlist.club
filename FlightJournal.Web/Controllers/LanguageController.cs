using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Controllers
{
    public class LanguageController : Controller
    {
        public ActionResult Set(string languageIsoCode, string returnUrl)
        {
            if (!Internationalization.Localizations.ContainsKey(languageIsoCode.ToLower()))
            {
                ViewBag.LanguageIsoCode = languageIsoCode;
                return View("LanguageNotSupported");
            }

            if (!Url.IsLocalUrl(returnUrl))
            {
                ViewBag.InvalidRedirectUrl = returnUrl;
                return View("RedirectNotSupported");
            }

            Internationalization.LanguageCode = languageIsoCode;
            return Redirect(returnUrl);
        }
    }
}