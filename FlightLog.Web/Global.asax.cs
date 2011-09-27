namespace FlightLog
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.Routing;

    using FlightLog.Controllers;
    using FlightLog.Models;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            ////routes.MapRoute(
            ////    "Default", // Route name
            ////    "", // URL with parameters
            ////    new { controller = "Report", action = "Index" } // Parameter defaults
            ////);

            ////routes.MapRoute(
            ////    "Default", // Route name
            ////    "", // URL with parameters
            ////    new { controller = "Report", action = "Index" } // Parameter defaults
            ////);

            ////routes.MapRoute(
            ////  "Default", // Route name
            ////  "{controller}/{action}/{id}", // URL with parameters
            ////    new { controller = "Flight", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            ////    );

            ////routes.MapRoute(
            ////  "Report", // Route name
            ////  "{controller}/{action}/{id}", // URL with parameters
            ////  new { controller = "Flight", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            ////);

            routes.MapRoute(
                "Home", // Route name
                "", // URL with parameters
                new { controller = "Report", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "ReportPage", // Route name
                "{date}", // URL with parameters
                new { controller = "Report", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                       "Controller_Action", // Route name
                       "{controller}/{action}/{id}", // URL with parameters
                       new { id = UrlParameter.Optional } // Parameter defaults
            );

            foreach (var route in GetDefaultRoutes())
            {
                routes.Add(route);
            }

            
        }

        private static IEnumerable<Route> GetDefaultRoutes()
        {
            //My controllers assembly (can be get also by name)
            Assembly assembly = typeof(HomeController).Assembly;
            // get all the controllers that are public and not abstract
            var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Controller)) && t.IsPublic && !t.IsAbstract);
            // run for each controller type
            foreach (var type in types)
            {
                //Get the controller name - each controller should end with the word Controller
                string controller = type.Name.Substring(0, type.Name.IndexOf("Controller"));
                // create the default
                RouteValueDictionary routeDictionary = new RouteValueDictionary
                                                   {
                                                       {"controller", controller}, // the controller name
                                                       {"action", "index"} // the default method
                                                   };
                yield return new Route(controller, routeDictionary, new MvcRouteHandler());
            }
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //using a custom initializer to seed the database,look in blogcontext to see
            //how it works
            ////System.Data.Entity.Database.SetInitializer(new FlightContext.FlightContextInitializer());
        }
    }
}