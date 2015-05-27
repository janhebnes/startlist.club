using System.Collections.Generic;
using System.IO;
using System.Web;

namespace FlightJournal.Web.Translations
{
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
            public static string LanguageCodeSessionKey = "LanguageCode";

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

        #endregion

    }
}
