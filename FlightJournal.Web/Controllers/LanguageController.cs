using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public class UserLanguages
        {
            public static readonly string[] SupportedLanguageIsoCodes = new string[] { "da", "en" };

            public static void SetPreferedLanguage(string languageIsoCode)
            {
                if (UserLanguages.SupportedLanguageIsoCodes.Contains(languageIsoCode))
                {
                    HttpCookie cookie = new HttpCookie("DefaultLanguage");
                    cookie.Value = languageIsoCode.ToLower();
                    cookie.Expires = DateTime.Now.AddMonths(36);
                    System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }

            public static string DefaultLanguage()
            {
                if (System.Web.HttpContext.Current.Items["DefaultLanguage"] != null)
                    return System.Web.HttpContext.Current.Items["DefaultLanguage"] as string;

                // Fetch a cookie value if set
                if (System.Web.HttpContext.Current.Request.Cookies["DefaultLanguage"] != null)
                {
                    if (UserLanguages.SupportedLanguageIsoCodes.Contains(System.Web.HttpContext.Current.Request.Cookies["DefaultLanguage"].Value))
                    {
                        System.Web.HttpContext.Current.Items["DefaultLanguage"] = System.Web.HttpContext.Current.Request.Cookies["DefaultLanguage"].Value.ToLower();
                        return System.Web.HttpContext.Current.Items["DefaultLanguage"] as string;
                    }
                }

                // Use browser user languages 
                foreach (var lang in GetRequestUserLanguages())
                {
                    if (SupportedLanguageIsoCodes.Contains(lang.IsoCode))
                    {
                        System.Web.HttpContext.Current.Items["DefaultLanguage"] = lang.IsoCode.ToLower();
                        return lang.IsoCode;
                    }
                    foreach (var iso in SupportedLanguageIsoCodes)
                    {
                        if (lang.IsoCode.StartsWith(iso))
                        {
                            System.Web.HttpContext.Current.Items["DefaultLanguage"] = iso.ToLower();
                            return iso;
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// UserLanguages in User Browser Request.
            /// </summary>
            /// <returns>Parsed Browser User Languages ordered by Weight</returns>
            public static IEnumerable<UserLanguage> GetRequestUserLanguages()
            {
                if (System.Web.HttpContext.Current == null) return null;
                if (System.Web.HttpContext.Current.Request.UserLanguages == null) return null;

                return System.Web.HttpContext.Current.Request.UserLanguages.Select(
                            d => new UserLanguage { IsoCode = ParseUserLanguage(d), Weight = ParseUserLanguageWeight(d) }).
                            OrderByDescending(l => l.Weight);
            }

            /// <summary>
            /// Parse the language part of the userlanguage element
            /// </summary>
            /// <example>
            /// E.g. UserLanguages en-US / en;q=0.8 / da;q=0.6 / de;q=0.4 / de-DE;q=0.2 / de-CH;q=0.2
            /// </example>
            /// <param name="s">userLanguage element</param>
            /// <returns>IsoCode</returns>
            private static string ParseUserLanguage(string s)
            {
                return !s.Contains(";q=") ? s : s.Substring(0, s.IndexOf(";q="));
            }

            /// <summary>
            /// Find the weight of the userlanguage 
            /// </summary>
            /// <example>
            /// E.g. UserLanguages en-US / en;q=0.8 / da;q=0.6 / de;q=0.4 / de-DE;q=0.2 / de-CH;q=0.2
            /// </example>
            /// <remarks>comes with a potential ";q=0.8" of none is specified it is assumed as 1 </remarks>
            /// <param name="s">userLanguage element</param>
            /// <returns>the weight / priority from 1 to 0</returns>
            private static double ParseUserLanguageWeight(string s)
            {
                if (!s.Contains(";q="))
                {
                    return 10;
                }

                double o;
                var q = s.Substring(s.IndexOf(";q=") + 3);
                return double.TryParse(q, out o) ? o : 10;
            }

            public class UserLanguage
            {
                public string IsoCode
                {
                    get;
                    set;
                }

                public double Weight
                {
                    get;
                    set;
                }
            }

        }

    }
}