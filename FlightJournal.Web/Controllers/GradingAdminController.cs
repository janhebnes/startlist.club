using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class GradingAdminController : Controller
    {
        private FlightContext db = new FlightContext();
        public ActionResult Index()
        {
            var model = db.Gradings;
            ViewBag.CanDelete = model.ToDictionary(x => x.GradingId, x => !IsInUse(x.GradingId));
            return View(model);
        }


        public ViewResult Details(int id)
        {
            var model = db.Gradings.Find(id);
            return View(model);
        }

        public ActionResult Create()
        {
            var model= new Grading();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(Grading model)
        {
            if (ModelState.IsValid)
            {
                db.Gradings.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.Gradings.Find(id);
            ViewBag.IsInUse = IsInUse(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Grading model)
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
            var model = db.Gradings.Find(id);
            return View(model);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.Gradings.Find(id);
            db.Gradings.Remove(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult SwapOrder(int itemId1, int itemId2)
        {
            var item1 = db.Gradings.FirstOrDefault(x => x.GradingId== itemId1);
            var item2 = db.Gradings.FirstOrDefault(x => x.GradingId == itemId2);
            if (item1 != null && item2!= null)
            {
                var tmp = item1.DisplayOrder;
                item1.DisplayOrder = item2.DisplayOrder;
                item2.DisplayOrder = tmp;
                db.Entry(item1).State = EntityState.Modified;
                db.Entry(item2).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private bool IsInUse(int id)
        {
            //TODO
            return false;
            //var isInUse = db.TrainingFlightAnnotations.SelectMany(x=>x.Manouvres).Any(y=>y.ManouvreId == id);
            //return isInUse;
        }


    }
}