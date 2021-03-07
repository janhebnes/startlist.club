using System;
using System.Web.Mvc;

namespace FlightJournal.Web.Translations
{
    /// <summary>
    /// Insert into Views\Web.config section system.web.webPages.razor override pages pageBaseType with FlightJournal.Web.Translations.LocalizedWebViewPage
    /// </summary>
    public abstract class LocalizedWebViewPage<TModel> : WebViewPage<TModel>
    {
        /// <summary>
        /// Localized Translation (i18n)
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        /// <example>@_("Country")</example>
        public string _(string en)
        {
            var s = Internationalization.GetText(en, Internationalization.LanguageCode);
            if (s != " ")
                return s.Trim();
            return s;
        }

        /// <summary>
        /// Localized Translation (i18n) wrapped in ping for use on razor attributes
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        /// <example><div class="col-xs-12" data-intro=@Html.Raw(__("Country") /></example>
        public string __(string en)
        {
            return "\"" + _(en) + "\"";
        }

        /// <summary>
        /// Localized Translation (i18n) String format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <example>@_("Benyttes på {0} registreringer."), usedCount)</example>
        public string _(string format, params Object[] args)
        {
            if (format == null || args == null)
                throw new ArgumentNullException((format == null) ? "format" : "args");

            return string.Format(Internationalization.GetText(format, Internationalization.LanguageCode), args);
        }


        public string DataTableLocalizationUrl()
        {
            switch (Internationalization.LanguageCode)
            {
                case "da": return "https://cdn.datatables.net/plug-ins/1.10.22/i18n/Danish.json";
                case "no": return "https://cdn.datatables.net/plug-ins/1.10.22/i18n/Norwegian-Bokmal.json";
                case "se": return "https://cdn.datatables.net/plug-ins/1.10.22/i18n/Swedish.json";
            }

            return null;
        }
}
}
