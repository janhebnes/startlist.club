using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FlightJournal.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
#if RELEASE
            ApplicationConfiguration.Config = AppConfig.RELEASE;
#elif DEMO
            ApplicationConfiguration.Config = AppConfig.DEMO;
#elif DEV
            ApplicationConfiguration.Config = AppConfig.DEV;
#elif DEBUG
            ApplicationConfiguration.Config = AppConfig.DEBUG;
#endif
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public static class ApplicationConfiguration
    {
        public static AppConfig Config { get; set; } = AppConfig.UNKNOWN;
    }

    public enum AppConfig
    {
        UNKNOWN,
        RELEASE,
        DEBUG,
        DEMO,
        DEV,
    }
}