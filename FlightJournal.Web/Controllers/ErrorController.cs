using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
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