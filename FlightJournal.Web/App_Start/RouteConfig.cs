using System.Linq.Expressions;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Routing;
using FlightJournal.Web.Constraints;
using FlightJournal.Web.Validators;

namespace FlightJournal.Web{
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Custom club Urls
            routes.MapRoute(
                "RootClub", // Route name
                "{club}", // URL with parameters
                new { controller = "Report", action = "Index" },
                new { club = new ClubRouteConstraint(new ClubValidator()) }
            );

            // Custom report url /{yyyy} or /{yyyy-MM} or startlist on date {yyyy-MM-dd}
            routes.MapRoute(
                "ReportingDate", // Route name
                "{date}", // URL with parameters
                new { controller = "Report", action = "Index" },
                new { date = new DateRouteConstraint(new DatePathValidator()) }
            );

            // Custom club Urls with custom Date
            routes.MapRoute(
                 "ReportingClubDate", // Route name
                 "{club}/{date}", // URL with parameters
                 new { controller = "Report", action = "Index" },
                 new { club = new ClubRouteConstraint(new ClubValidator())
                     , date = new DateRouteConstraint(new DatePathValidator()) }
             );

            // Custom club Urls with default behavior
            routes.MapRoute(
                "DefaultWithClubFlavor", // Route name
                "{club}/{controller}/{action}/{id}", // URL with parameters
                new { club = UrlParameter.Optional, controller = "Report", action = "Index", id = UrlParameter.Optional },
                new { club = new ClubRouteConstraint(new ClubValidator()) }
            );

            // Default behaviour
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Report", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}