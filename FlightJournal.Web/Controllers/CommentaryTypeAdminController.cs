using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class CommentaryTypeAdminController : Controller
    {
        private FlightContext db = new FlightContext();
        public ActionResult Index()
        {
            var model = db.CommentaryTypes;
            ViewBag.CanDelete = model.ToDictionary(x => x.CommentaryTypeId, x => !IsCommentaryTypeInUse(x.CommentaryTypeId));
            return View(model);
        }

        public ActionResult Create()
        {
            var commentary= new CommentaryType();
            return View(commentary);
        }
        [HttpPost]
        public ActionResult Create(CommentaryType item)
        {
            if (ModelState.IsValid)
            {
                db.CommentaryTypes.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(item);
        }

        public ActionResult Edit(int id)
        {
            var item = db.CommentaryTypes.Find(id);
            ViewBag.IsInUse = IsCommentaryTypeInUse(id);
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Commentary commentary)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commentary).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(commentary);
        }


        public ActionResult Delete(int id)
        {
            var commentary = db.Commentaries.Find(id);
            return View(commentary);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.CommentaryTypes.Find(id);
            db.CommentaryTypes.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        private bool IsCommentaryTypeInUse(int id)
        {
            var inUse = db.TrainingFlightAnnotations.SelectMany(x => x.TrainingFlightAnnotationCommentCommentTypes)
                .Any(y => y.CommentaryTypeId == id);

            return inUse;
        }
    }
}