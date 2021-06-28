using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using FlightJournal.Web.Logging;
using Newtonsoft.Json;

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

        private static AutoExportConfigurationSection AutoExport
        {
            get
            {
                /// Custom Config section for allowing hiding of the values 
                var settings = ConfigurationManager.GetSection("autoExportSection") as AutoExportConfigurationSection ?? new AutoExportConfigurationSection();
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


        public static IEnumerable<AutoExport> AutoExports
        {
            get
            {
                try
                {
                    var azureSetting = ConfigurationManager.AppSettings["AutoExport"];
                    if (azureSetting != null)
                    {
                        var res = JsonConvert.DeserializeObject<List<AutoExport>>(azureSetting);
                        return res;
                    }
                    else
                    {
                        var section = ConfigurationManager.GetSection("autoExportSection") as AutoExportConfigurationSection;
                        var res = new List<AutoExport>();
                        foreach (var x in section.AutoExports)
                        {
                            var aece = x as AutoExportConfigElement;
                            if (aece != null)
                            {
                                var export = new AutoExport { Name = aece.Name, Username = aece.Username, Password = aece.Password, TokenUrl = aece.TokenUrl, PostUrl = aece.PostUrl, IntervalInMinutes = aece.IntervalMinutes};
                                res.Add(export);
                            }
                        }

                        return res;
                    }
                }
                catch (Exception e)
                {
                    Log.Warning($"Unable to parse azure setting for autoexport: {e.Message}");
                }
                return Enumerable.Empty<AutoExport>();
            }
        }
    }
}