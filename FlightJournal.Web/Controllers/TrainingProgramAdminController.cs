using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TrainingProgramAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        public ActionResult Index(int dtoId=-1)
        {
            var items = db.TrainingPrograms;
            return View(items);
        }

        public ViewResult Details(int id)
        {
            var item= db.TrainingPrograms.Find(id);
            return View(item);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Training2Program program)
        {
            if (ModelState.IsValid)
            {
                db.TrainingPrograms.Add(program);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(program);
        }

        public ActionResult Edit(int id)
        {
            var item = db.TrainingPrograms.Find(id);
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Training2Program program)
        {
            if (ModelState.IsValid)
            {
                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(program);
        }


        public ActionResult Delete(int id)
        {
            var program = db.TrainingPrograms.Find(id);


            return View(program);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.TrainingPrograms.Find(id);
            db.TrainingPrograms.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}