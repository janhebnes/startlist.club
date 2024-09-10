using FlightJournal.Web.Constraints;
using FlightJournal.Web.Repositories;
using FlightJournal.Web.Validators;
using System.Web.Mvc;
using System.Web.Routing;

namespace FlightJournal.Web
{
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes, IClubRepository clubRepository, bool unittest = false) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            if (!unittest)
                routes.MapMvcAttributeRoutes();

            // Specific routes to avoid HttpException with The controller for path e.g. '/.well-known/traffic-advice' was not found or does not implement IController.
            // Array of URLs to handle as 404
            string[] notFoundUrls = {
                "NaN-NaN-NaN/Index",
                "sql/Index",
                ".well-known/traffic-advice",
                ".well-known/apple-app-site-association",
                "app/Index",
                "actuator;/env;",
                "Admin/ashx",
                "old/Index",
                "root/Index",
                "'123/Index",
                "includ/Index",
                "wordpress/Index",
                "actuator/heapdump",
                "apple-app-site-association/Index" // Assuming no Apple app association
            };
            foreach (var url in notFoundUrls) // Generate routes for known 404 URLs - avoiding validation in ClubRouteConstraint 
            {
                routes.MapRoute(
                    name: url.Replace("/", "").Replace(";", ""), // Generate a unique name for each route
                    url: url,
                    defaults: new { controller = "Error", action = "PageNotFound" }
                );
            }

            // Custom club Urls
            routes.MapRoute(
                name: "GetReportByClub",
                url: "{club}", 
                defaults: new { controller = "Report", action = "Index" },
                constraints: new { club = new ClubRouteConstraint(new ClubValidator(clubRepository)), controller = "Report" }
            );

            // Custom report url /{yyyy} or /{yyyy-MM} or startlist on date {yyyy-MM-dd}
            routes.MapRoute(
                name: "GetReportByDate", 
                url: "{date}", 
                defaults: new { controller = "Report", action = "Index" },
                constraints: new { date = new DateRouteConstraint(new DatePathValidator()), controller = "Report" }
            );

            // Custom club Urls with custom Date
            routes.MapRoute(
                 name: "GetReportByClubByDate", 
                 url: "{club}/{date}",
                 defaults: new { controller = "Report", action = "Index" },
                 constraints: new { club = new ClubRouteConstraint(new ClubValidator(clubRepository)), date = new DateRouteConstraint(new DatePathValidator()), controller = "Report" }
             );

            // Custom club Urls with default behavior
            routes.MapRoute(
                name: "GetDefaultByClub", // Route name
                url: "{club}/{controller}/{action}/{id}", // URL with parameters
                defaults: new { club = UrlParameter.Optional, controller = "Report", action = "Index", id = UrlParameter.Optional },
                constraints: new { club = new ClubRouteConstraint(new ClubValidator(clubRepository)), controller = "Report|About|Account|Club|CodePlayGround|CommentaryAdmin|CommentaryTypeAdmin|Error|Flight|GradingAdmin|Import|Language|Location|Logbook|Manage|ManouvreAdmin|Pilot|PilotStatus|Plane|Report|RolesAdmin|StartType|TrainingExerciseAdmin|TrainingLessonAdmin|TrainingLogAdmin|TrainingLogHistoryAdmin|TrainingProgramAdmin|TrainingStatus|UsersAdmin" } // Constraint to club and all known controllers
            );

            // Default behaviour
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Report", action = "Index", id = UrlParameter.Optional },
                constraints: new { controller = "Report|About|Account|Club|CodePlayGround|CommentaryAdmin|CommentaryTypeAdmin|Error|Flight|GradingAdmin|Import|Language|Location|Logbook|Manage|ManouvreAdmin|Pilot|PilotStatus|Plane|Report|RolesAdmin|StartType|TrainingExerciseAdmin|TrainingLessonAdmin|TrainingLogAdmin|TrainingLogHistoryAdmin|TrainingProgramAdmin|TrainingStatus|UsersAdmin" } // Constraint to all known controllers
            );

            // Catch-all route for unmatched URLs
            routes.MapRoute(
                name: "NotFound",
                url: "{*url}", // Matches any unmatched URL
                defaults: new { controller = "Error", action = "PageNotFound" }
            );
        }
    }
}