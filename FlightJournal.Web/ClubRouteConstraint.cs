using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace FlightJournal.Web
{
    public class ClubRouteConstraint : IRouteConstraint
    {
        private readonly IClubValidator clubValidator;

        public ClubRouteConstraint(IClubValidator clubValidator)
        {
            this.clubValidator = clubValidator;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object club = null;

            values.TryGetValue("club", out club);

            return clubValidator.IsValid(club as string);
        }
    }
}