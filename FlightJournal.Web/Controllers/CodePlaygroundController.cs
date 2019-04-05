using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    [Authorize]
    [RoutePrefix("Book-Reviews")]
    public class CodePlaygroundController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Administration()
        {
            return View();
        }
        
        public ActionResult License()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult TableExpand()
        {
            return View();
        }

        [AllowAnonymous]
        public PartialViewResult TableExpandPartial()
        {
            return this.PartialView();
        }

    }
}
