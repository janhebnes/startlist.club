using System.Web;
using System.Web.Routing;
using FlightJournal.Web.Validators;

namespace FlightJournal.Web.Constraints
{
    public class DateRouteConstraint : IRouteConstraint
    {
        private readonly IDateRouteValidator dateRouteValidator;

        public DateRouteConstraint(IDateRouteValidator dateRouteValidator)
        {
            this.dateRouteValidator = dateRouteValidator;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object date = null;

            values.TryGetValue("date", out date);

            return dateRouteValidator.IsValid(date as string);
        }
    }
}