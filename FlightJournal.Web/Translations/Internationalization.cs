using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace FlightJournal.Web.Translations
{
    /// <summary>
    /// Code base from http://www.fairtutor.com/fairlylocal/ project
    /// Inspired to use GETTEXT by the Python Babel on https://github.com/skylines-project/skylines/blob/master/skylines/frontend/translations/messages.pot 
    /// </summary>
    /// <remarks>
    /// Inspired by the model used on https://github.com/skylines-project/skylines/blob/master/skylines/frontend/translations/messages.pot 
    /// I researched more on how to get pot files in use on this project. By using the NuGet package from https://github.com/vslavik/gettext-tools-windows/releases 
    /// installed and the sample postbuild from http://www.fairtutor.com/fairlylocal/ to create my own translation.cmd in solution root. 
    /// We can not generate pot file and merge po files.
    /// Other inspiring sources:
    /// http://www.expatsoftware.com/articles/2010/03/why-internationalization-is-hopelessly.html
    /// https://github.com/fsateler/gettext-cs-utils and 
    /// http://manas.com.ar/blog/2009/10/01/using-gnu-gettext-for-i18n-in-c-and-asp-net.html
    /// </remarks>
    public static class Internationalization
    {
        public static class Settings
        {
            /// <summary>
            /// The language that MsgIDs are given in (generally the language the project is being developed in).
            /// </summary>
            public static string WorkingLanguage = "en";

            /// <summary>
            /// This is the base path under which localizations will 
            /// be stored.
            /// </summary>
            public static string BasePath = "~/Translations/";

            /// <summary>
            /// The session key that will be used to store a user's language preference
            /// </summary>
            public static string LanguageCodeCookieKey = "LanguageCode";

            /// <summary>
            /// FOR TESTING ONLY!  If true, all calls to GetText will return an empty string.
            /// this is useful when searching for any strings that might not be flagged for localization.
            /// </summary>
            public static bool HideAllLocalizedText;
        }

        private static string _basePathAbsolute;
        public static Dictionary<string, Localization> Localizations;

        static Internationalization()
        {
            Localizations = new Dictionary<string, Localization>();
            _basePathAbsolute = HttpContext.Current.Server.MapPath(Settings.BasePath).ToLower();

            if (!Directory.Exists(_basePathAbsolute))
            {
                Directory.CreateDirectory(_basePathAbsolute);
            }

            foreach (string dir in Directory.GetDirectories(_basePathAbsolute))
            {
                string lcPath = Path.Combine(dir, "LC_MESSAGES");
                if (!Directory.Exists(lcPath))
                {
                    continue;
                }

                Localization l = new Localization();
                foreach (string filename in Directory.GetFiles(lcPath, "*.po"))
                {
                    l.LoadFromFile(filename);
                }

                if (l.Messages.Count > 0)
                {
                    Localizations.Add(dir.Replace(_basePathAbsolute, "").ToLower(), l);
                }
            }

            if (!Localizations.ContainsKey(Settings.WorkingLanguage))
            {
                Localizations.Add(Settings.WorkingLanguage, new Localization());
            }
        }

        #region public methods

        /// <summary>
        /// Get a translated version of the supplied text message.
        /// </summary>
        /// <param name="msgID">Text to be translated</param>
        /// <param name="languageCode">Language to translate into</param>
        /// <returns></returns>
        public static string GetText(string msgID, string languageCode)
        {
            if (Settings.HideAllLocalizedText)
            {
                return "";
            }

            if (!Localizations.ContainsKey(languageCode.ToLower()))
            {
                languageCode = Settings.WorkingLanguage;
            }

            return Localizations[languageCode.ToLower()].GetMessage(msgID);
        }

        /// <summary>
        /// Spins through the language preferences in the supplied HttpRequest,
        /// returning the first complete or partial match on a loaded language.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static string GetBestLanguage(HttpRequest request, string fallback)
        {
            if (request.UserLanguages == null || request.UserLanguages.Length == 0)
            {
                return Settings.WorkingLanguage;
            }

            foreach (string lang in request.UserLanguages)
            {
                if (Localizations.ContainsKey(lang.ToLower()))
                {
                    return lang;
                }

                string fragment = lang.ToLower().Split('-')[0];
                if (Localizations.ContainsKey(fragment))
                {
                    return fragment;
                }

            }

            return fallback;
        }

        /// <summary>
        /// Get the current user's language preference.
        /// First looks for stored preference in the Session.
        /// Falls back on the best match from the browser's language collection.
        /// </summary>
        public static string LanguageCode
        {
            get
            {
                // LocalizedDisplayNameAttribute is run from applicationstart and without context. 
                if (HttpContext.Current == null)
                    return "en"; 

                if (HttpContext.Current.Items[Settings.LanguageCodeCookieKey] != null)
                    return HttpContext.Current.Items[Settings.LanguageCodeCookieKey] as string;

                // Fetch from cookie value if set
                if (HttpContext.Current.Request.Cookies[Settings.LanguageCodeCookieKey] != null)
                {
                    if (Localizations.ContainsKey(HttpContext.Current.Request.Cookies[Settings.LanguageCodeCookieKey].Value))
                    {
                        HttpContext.Current.Items[Settings.LanguageCodeCookieKey] = HttpContext.Current.Request.Cookies[Settings.LanguageCodeCookieKey].Value.ToLower();
                        return HttpContext.Current.Items[Settings.LanguageCodeCookieKey] as string;
                    }
                }

                // Select from browser
                HttpContext.Current.Items[Settings.LanguageCodeCookieKey] = GetBestLanguage(HttpContext.Current.Request, Settings.WorkingLanguage);

                return HttpContext.Current.Items[Settings.LanguageCodeCookieKey] as string;
            }
            set
            {
                if (Localizations.ContainsKey(value.ToLower()))
                {
                    HttpCookie cookie = new HttpCookie(Settings.LanguageCodeCookieKey);
                    cookie.Value = value.ToLower();
                    cookie.Expires = DateTime.Now.AddMonths(36);
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
                HttpContext.Current.Items[Settings.LanguageCodeCookieKey] = value.ToLower();
            }
        }

        #endregion

    }
}
