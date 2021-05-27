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
            ConfigureOpenGliderNetworkFlightLogMonitor(app);
            ConfigureAprsDataListener();
            app.MapSignalR();
        }
    }
}
