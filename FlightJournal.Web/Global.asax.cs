using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Boerman.AprsClient;
using FlightJournal.Web.App_Start;
using WebGrease.Configuration;

namespace FlightJournal.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
#if CFG_RELEASE
            ApplicationConfiguration.Config = AppConfig.CFG_RELEASE;
#elif CFG_DEMO
            ApplicationConfiguration.Config = AppConfig.CFG_DEMO;
#elif CFG_DEV
            ApplicationConfiguration.Config = AppConfig.CFG_DEV;
#elif CFG_DEBUG
            ApplicationConfiguration.Config = AppConfig.CFG_DEBUG;
#endif
            log4net.Config.XmlConfigurator.Configure();
            log4net.GlobalContext.Properties["CONFIG"] = ApplicationConfiguration.Config;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public static class ApplicationConfiguration
    {
        public static AppConfig Config { get; set; } = AppConfig.CFG_UNKNOWN;
    }

    public enum AppConfig
    {
        CFG_UNKNOWN,
        CFG_RELEASE,
        CFG_DEBUG,
        CFG_DEMO,
        CFG_DEV,
    }
}