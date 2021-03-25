using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CommentaryTypeAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();
        public ActionResult Index()
        {
            var model = db.CommentaryTypes;
            if (model.Count(x => x.DisplayOrder == 0) > 1)
            {
                // not yet defined, set order according to current implicit order. You could argue that this should be done elsewhere...
                var order = 0;
                foreach (var m in model)
                {
                    m.DisplayOrder = order++;
                    db.Entry(m).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
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
        public ActionResult Edit(CommentaryType item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(item);
        }


        public ActionResult Delete(int id)
        {
            var item = db.CommentaryTypes.Find(id);
            return View(item);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.CommentaryTypes.Find(id);
            db.CommentaryTypes.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult SwapOrder(int itemId1, int itemId2)
        {
            var item1 = db.CommentaryTypes.FirstOrDefault(x => x.CommentaryTypeId == itemId1);
            var item2 = db.CommentaryTypes.FirstOrDefault(x => x.CommentaryTypeId == itemId2);
            if (item1 != null && item2 != null)
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


        private bool IsCommentaryTypeInUse(int id)
        {
            var inUse = db.TrainingFlightAnnotations.SelectMany(x => x.TrainingFlightAnnotationCommentCommentTypes)
                .Any(y => y.CommentaryTypeId == id);

            return inUse;
        }
    }
}