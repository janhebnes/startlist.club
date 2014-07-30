using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FlightJournal.Web
{
    public class AuthenticationConfigurationSection : ConfigurationSection
    {
        private AuthenticationConfigurationSection() { }

        [ConfigurationProperty("FacebookAppId", DefaultValue = "")]
        public string FacebookAppId
        {
            get { return (string)this["FacebookAppId"]; }
            set { this["FacebookAppId"] = value; }
        }

        [ConfigurationProperty("FacebookAppSecret", DefaultValue = "")]
        public string FacebookAppSecret
        {
            get { return (string)this["FacebookAppSecret"]; }
            set { this["FacebookAppSecret"] = value; }
        }

        [ConfigurationProperty("GoogleClientId", DefaultValue = "")]
        public string GoogleClientId
        {
            get { return (string)this["GoogleClientId"]; }
            set { this["GoogleClientId"] = value; }
        }

        [ConfigurationProperty("GoogleClientSecret", DefaultValue = "")]
        public string GoogleClientSecret
        {
            get { return (string)this["GoogleClientSecret"]; }
            set { this["GoogleClientSecret"] = value; }
        }

    }
}