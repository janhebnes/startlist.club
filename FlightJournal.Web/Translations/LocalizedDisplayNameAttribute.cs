using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJournal.Web.Translations
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string _displayName;

        public LocalizedDisplayNameAttribute(string resourceId)
            : base(resourceId)
        {
            _displayName = resourceId;
        }

        private static string GetMessageFromResource(string resourceId)
        {
            return Internationalization.GetText(resourceId, Internationalization.LanguageCode);
        }

        public override string DisplayName
        {
            get
            {
                return GetMessageFromResource(_displayName);
            }
        }
    }
}
