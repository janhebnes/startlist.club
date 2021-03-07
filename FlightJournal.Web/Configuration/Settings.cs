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
    }
}