using System.Web.Mvc;
using System.Web.Routing;

namespace FlightJournal.Web{
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Allowing the reports to be visible from the root addresses /{year} or /{year}/{month}
            routes.MapRoute(
                "ReportPage", // Route name
                "{date}", // URL with parameters
                new { controller = "Report", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}