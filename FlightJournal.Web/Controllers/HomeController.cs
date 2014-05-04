using System.Web.Mvc;

namespace FlightJournal.Web.Controllers{
    public class HomeController : Controller {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
