using System.Linq.Expressions;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Routing;

namespace FlightJournal.Web{
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // For allowing reports to be visible from the root addresses /{year} or /{year}/{month} and browsing startlists using {date yyyy-mm-dd}
            // We must map all Index actions otherwise it will be grabed by the ReportPage routemap
            routes.MapRoute(
                "Account", // Route name
                "Account", // URL with parameters
                new { controller = "Account", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Club", // Route name
                "Club", // URL with parameters
                new { controller = "Club", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "Flight", // Route name
                "Flight", // URL with parameters
                new { controller = "Flight", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "Home", // Route name
                "Home", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Location", // Route name
                "Location", // URL with parameters
                new { controller = "Location", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Manage", // Route name
                "Manage", // URL with parameters
                new { controller = "Manage", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Pilot", // Route name
                "Pilot", // URL with parameters
                new { controller = "Pilot", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "PilotStatus", // Route name
                "PilotStatus", // URL with parameters
                new { controller = "PilotStatus", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Plane", // Route name
                "Plane", // URL with parameters
                new { controller = "Plane", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "RolesAdmin", // Route name
                "RolesAdmin", // URL with parameters
                new { controller = "RolesAdmin", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "StartType", // Route name
                "StartType", // URL with parameters
                new { controller = "StartType", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "UsersAdmin", // Route name
                "UsersAdmin", // URL with parameters
                new { controller = "UsersAdmin", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            // ---------------------------
            // Allowing the reports to be visible from the root addresses /{year} or /{year}/{month} and browsing startlists using {date yyyy-mm-dd}

            routes.MapRoute(
                "RootClub", // Route name
                "{club}", // URL with parameters
                new { controller = "Report", action = "Index" },
                new { club = new ClubRouteConstraint(new ClubValidator()) } 
            );

            routes.MapRoute(
                "ReportingDate", // Route name
                "{date}", // URL with parameters
                new { controller = "Report", action = "Index" },
                new { date = new DateRouteConstraint(new DatePathValidator()) }
            );

           routes.MapRoute(
                "ReportingClubDate", // Route name
                "{club}/{date}", // URL with parameters
                new { controller = "Report", action = "Index"},
                new { club = new ClubRouteConstraint(new ClubValidator()), date = new DateRouteConstraint(new DatePathValidator()) } 
            );
            
            routes.MapRoute(
                "DefaultWithClubFlavor", // Route name
                "{club}/{controller}/{action}/{id}", // URL with parameters
                new { club = UrlParameter.Optional, controller = "Report", action = "Index", id = UrlParameter.Optional },
                new { club = new ClubRouteConstraint(new ClubValidator()) } 
            );

            // Default map
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Report", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}