using Owin;
using Microsoft.Azure.Services.AppAuthentication;

namespace FlightJournal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable SQL command tracing on sql commands that fail in entity framework
            System.Data.Entity.Infrastructure.Interception.DbInterception.Add(new App_Start.DbCommandInterceptor());

            // will look for SQL connection strings that contain Authentication=Active Directory Interactive. When found, they will use the AzureServiceTokenProvider to fetch an access token to authenticate with Azure SQL Database.
            // https://www.pluralsight.com/guides/how-to-use-managed-identity-with-azure-sql-database
            System.Data.SqlClient.SqlAuthenticationProvider.SetProvider(System.Data.SqlClient.SqlAuthenticationMethod.ActiveDirectoryInteractive, new SqlAppAuthenticationProvider());

            ConfigureAuth(app);
            ConfigureSmsLogin();
            ConfigureAprsDataListener();
            app.MapSignalR();
        }

        /// <summary>
        /// Resolve the SMS configuration once at startup and log how SMS login is set up: mode and region, and for a
        /// misconfiguration the setting names involved. Never a phone number, SID, key or token.
        /// </summary>
        private static void ConfigureSmsLogin()
        {
            // Startup never depends on SMS: whatever goes wrong here, SMS is simply unavailable and the site starts
            var line = SmsLoginStartupLine(() => FlightJournal.Web.Configuration.Settings.TwilioSms);
            try
            {
                System.Diagnostics.Trace.TraceInformation(line);
                new Microsoft.ApplicationInsights.TelemetryClient().TrackTrace(line);
            }
            catch (System.Exception)
            {
                // Logging must not stop startup either
            }
        }

        /// <summary>
        /// The startup line for SMS login. Never throws: a failure while resolving is reported by exception type only,
        /// never its message (which could contain configured values).
        /// </summary>
        internal static string SmsLoginStartupLine(System.Func<FlightJournal.Web.Configuration.TwilioSmsConfig> config)
        {
            try
            {
                return config().Describe();
            }
            catch (System.Exception ex)
            {
                return "SMS login: mode=Unavailable, problem=the SMS configuration could not be resolved (" + ex.GetType().Name + ")";
            }
        }
    }
}
