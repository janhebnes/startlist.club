using System.Net;
using System.Web.Mvc;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Controllers
{
    public class LanguageController : Controller
    {
        [Route("Language/Set")]
        [Route("{club}/Language/Set")]   // club pages emit club-prefixed links today; segment is ignored, never DB-validated
        public ActionResult Set(string languageIsoCode, string returnUrl)
        {
            // Reject before any work: only crawlers/probes reach this without a valid code
            if (!Internationalization.IsSupported(languageIsoCode))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unsupported language");

            Internationalization.LanguageCode = languageIsoCode.Trim();

            if (!Url.IsLocalUrl(returnUrl))
                return Redirect("/");

            return Redirect(returnUrl);
        }
    }
}
