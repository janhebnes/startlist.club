using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404; // Set the 404 status code
            return new ContentResult { Content = "<pre> ________________________________________ \r\n<br/>< 404 Page Not Found - <em>Udelanding</em>!  >\r\n<br/> ---------------------------------------- \r\n<br/>        \\   ^__^\r\n<br/>         \\  (oo)\\_______\r\n<br/>            (__)\\       )\\/\\\r\n<br/>                ||----w |\r\n<br/>                ||     ||\r\n<br/>⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⣽⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⠁⠀⢀⣾⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⣠⣾⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⠀⣠⣾⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⣻⣾⡍⠻⣿⣿⣿⠟⠁⠀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⡙⠁⠀⠈⠻⣧⡀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣦⡀⠀⠀⠈⠻⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠉⠻⣦⡀⠀⠀⠙⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⠀⣠⣾⣿⣷⣤⡀⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣿⣿⣿⣿⡿⠋⠁⠀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣦⡀⢈⣿⠟⠉⠉⢻⣿⣿⣿\r\n⣿⣿⣿⣿⣿⠏⠀⠀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⢀⣴⣿⣿⣿⣿\r\n⣿⣿⣿⠟⠁⠀⠀⣠⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⢀⣴⣿⣿⣿⣿⣿⣿\r\n⣿⣟⠁⠀⣠⣴⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⣿⣿⣿⣿⣿⣿⣿⣿\r\n⣿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿</pre>" }; // Return an empty ContentResult
        }

        public ActionResult PilotNotFound()
        {
            //Response.StatusCode = 404;
            return View();
        }

        public ActionResult Restricted(string message)
        {
            ViewBag.Message = message;
            return View();
        }

    // The server.GetLastError is currently not preserved... so the 500 handling is currently disabled
    ////    public ActionResult Runtime()
    ////    {
    ////        Response.StatusCode = 500;
    ////        return View();
    ////    }

    ////    public ActionResult TestRuntimeError()
    ////    {
    ////        throw new Exception("Testing Runtime Error - the server.GetLastError is currently not preserved... so the 500 handling is currently disabled");
    ////        return null;
    ////    }

    ////    public PartialViewResult Report()
    ////    {
    ////        return PartialView();
    ////    }

    ////    [HttpPost]
    ////    [ValidateInput(false)]
    ////    public ViewResult Submit()
    ////    {
    ////        //TODO: Transfer Request Values for Email submission
    ////        return View();
    ////    }
    }
}