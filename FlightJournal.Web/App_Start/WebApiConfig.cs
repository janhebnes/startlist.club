using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;

namespace FlightJournal.Web.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            //config.Routes.MapHttpRoute(
            //    name: "Swagger UI",
            //    routeTemplate: "api",
            //    defaults: null,
            //    constraints: null,
            //    new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "documentation/index")
            //);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}",
                defaults: null
            );
        }
    }
}