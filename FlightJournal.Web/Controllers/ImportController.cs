using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    public class ImportController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (!requestContext.HttpContext.User.IsInRole("Administrator") && !requestContext.HttpContext.User.IsInRole("Editor") && !requestContext.RouteData.Values.ContainsValue("Create"))
            {
                throw new UnauthorizedAccessException(string.Format("Access Denied to User {0}", this.Request.RequestContext.HttpContext.User.Identity.Name));
            }
            base.Initialize(requestContext);
        }
        //
        // GET: /Import/
        public ActionResult Index()
        {
            
            return View();
        }
	}
}