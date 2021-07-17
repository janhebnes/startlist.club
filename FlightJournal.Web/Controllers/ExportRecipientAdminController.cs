using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ExportRecipientAdminController : Controller
    {
        private FlightContext db = new FlightContext();
        public ActionResult Index()
        {
            var model = db.ExportRecipients;
            ViewBag.CanDelete = model.ToDictionary(x => x.ExportRecipientId, x => !IsInUse(x.ExportRecipientId));
            return View(model);
        }


        public ViewResult Details(int id)
        {
            var model = db.ExportRecipients.Find(id);
            return View(model);
        }

        public ActionResult Create()
        {
            var model= new ExportRecipient();

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(ExportRecipient model)
        {
            if (ModelState.IsValid)
            {
                db.ExportRecipients.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.ExportRecipients.Find(id);
            ViewBag.IsInUse = IsInUse(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ExportRecipient model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            var model = db.ExportRecipients.Find(id);
            return View(model);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.ExportRecipients.Find(id);
            db.ExportRecipients.Remove(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private bool IsInUse(int id)
        {
            return db.Clubs.Any(c=>c.ExportRecipientId == id);
        }


    }
}