using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
namespace FlightJournal.Web.Configuration
{
    public static class Settings
    {
        /// The App_Data folder is used on live, demo and dev on the windows web hosted configuration for storing the actual environment credentials 
        private static ServiceCredentialsConfigurationSection ServiceCredentials
        {
            get
            {
                /// Custom Config section for allowing hiding of the values 
                var settings = ConfigurationManager.GetSection("serviceCredentials") as ServiceCredentialsConfigurationSection;
                if (settings == null)
                    throw new ConfigurationErrorsException("Missing ServiceCredentials section");
                return settings;
            }
        }
        
        public static string FacebookAppId 
        { 
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["FacebookAppId"] != null)
                    return ConfigurationManager.AppSettings["FacebookAppId"];

                return ServiceCredentials.FacebookAppId;
            }
        }
        public static string FacebookAppSecret 
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["FacebookAppSecret"] != null)
                    return ConfigurationManager.AppSettings["FacebookAppSecret"];

                return ServiceCredentials.FacebookAppSecret;
            }
        }
        public static string GoogleClientId
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["GoogleClientId"] != null)
                    return ConfigurationManager.AppSettings["GoogleClientId"];

                return ServiceCredentials.GoogleClientId;
            }
        }
        public static string GoogleClientSecret
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["GoogleClientSecret"] != null)
                    return ConfigurationManager.AppSettings["GoogleClientSecret"];

                return ServiceCredentials.GoogleClientSecret;
            }
        }

        public static string MicrosoftClientId
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MicrosoftClientId"] != null)
                    return ConfigurationManager.AppSettings["MicrosoftClientId"];

                return ServiceCredentials.MicrosoftClientId;
            }
        }
        public static string MicrosoftClientSecret
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MicrosoftClientSecret"] != null)
                    return ConfigurationManager.AppSettings["MicrosoftClientSecret"];

                return ServiceCredentials.MicrosoftClientSecret;
            }
        }

        public static string TwilioAccountSid
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["TwilioAccountSid"] != null)
                    return ConfigurationManager.AppSettings["TwilioAccountSid"];

                return ServiceCredentials.TwilioAccountSid;
            }
        }
        public static string TwilioAuthToken
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["TwilioAuthToken"] != null)
                    return ConfigurationManager.AppSettings["TwilioAuthToken"];

                return ServiceCredentials.TwilioAuthToken;
            }
        }
        public static string TwilioFromNumber
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["TwilioFromNumber"] != null)
                    return ConfigurationManager.AppSettings["TwilioFromNumber"];

                return ServiceCredentials.TwilioFromNumber;
            }
        }
        public static string MailSmtpHost
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MailSmtpHost"] != null)
                    return ConfigurationManager.AppSettings["MailSmtpHost"];

                var smtpSection = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                return smtpSection.Network.Host;
            }
        }
        public static int MailSmtpPort
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MailSmtpPort"] != null)
                {
                    if (int.TryParse(ConfigurationManager.AppSettings["MailSmtpPort"], out int port))
                        return port;
                }

                var smtpSection = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                return smtpSection.Network.Port;
            }
        }

        public static string MailSmtpUserName
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MailSmtpUserName"] != null)
                    return ConfigurationManager.AppSettings["MailSmtpUserName"];

                var smtpSection = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                return smtpSection.Network.UserName;
            }
        }
        public static string MailSmtpPassword
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MailSmtpPassword"] != null)
                    return ConfigurationManager.AppSettings["MailSmtpPassword"];

                var smtpSection = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                return smtpSection.Network.Password;
            }
        }
        public static string MailFrom
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["MailFrom"] != null)
                    return ConfigurationManager.AppSettings["MailFrom"];

                var smtpSection = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                return smtpSection.From;
            }
        }
        public static string ApiKey
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["ApiKey"] != null)
                    return ConfigurationManager.AppSettings["ApiKey"];

                return null;
            }
        }

        public static Tuple<DateTime, DateTime> AprsListenerActiveHours
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                var today = DateTime.Now.Date;
                if (ConfigurationManager.AppSettings["AprsListenerActiveHoursUTC"] != null)
                {
                    var hours = ConfigurationManager.AppSettings["AprsListenerActiveHoursUTC"].Split('-');
                    if (hours.Length == 2)
                    {
                        if (TimeSpan.TryParse(hours[0], out TimeSpan start) &&
                            TimeSpan.TryParse(hours[1], out TimeSpan end))
                        {
                            return new Tuple<DateTime, DateTime>(today+start, today+end);
                        }
                    }
                }

                return new Tuple<DateTime, DateTime>(today + TimeSpan.FromHours(8), today + TimeSpan.FromHours(21));
            }
        }

        public static int OgnCatalogRefreshIntervalInHours
        {
            get
            {
                // Azure Configured Environment AppSettings or fallback to the App_Data folder 
                if (ConfigurationManager.AppSettings["OgnCatalogRefreshIntervalInHours"] != null)
                {
                    if (int.TryParse(ConfigurationManager.AppSettings["OgnCatalogRefreshIntervalInHours"], out int interval))
                        return interval;
                }

                return 4;
            }
        }

    }
}