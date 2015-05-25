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
            if (!UserLanguages.SupportedLanguageIsoCodes.Contains(languageIsoCode))
            {
                ViewBag.LanguageIsoCode = languageIsoCode;
                return View("LanguageNotSupported");
            }
            
            UserLanguages.SetPreferedLanguage(languageIsoCode);
            return Redirect(returnUrl);
        }

        public ActionResult Translate()
        {
            return View();
        }
    }
}