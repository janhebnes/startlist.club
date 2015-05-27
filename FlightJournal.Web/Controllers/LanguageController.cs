using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            
            Internationalization.LanguageCode = languageIsoCode;
            return Redirect(returnUrl);
        }
    }
}