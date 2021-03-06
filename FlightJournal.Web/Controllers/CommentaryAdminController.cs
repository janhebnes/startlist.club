using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class CommentaryAdminController : Controller
    {
        private FlightContext db = new FlightContext();
        public ActionResult Index()
        {
            var model = db.Commentaries;
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
            ViewBag.CanDelete = model.ToDictionary(x => x.CommentaryId, x => !IsCommentaryInUse(x.CommentaryId));
            return View(model);
        }


        public ActionResult Create()
        {
            var commentary= new Commentary();
            return View(commentary);
        }
        [HttpPost]
        public ActionResult Create(Commentary commentary)
        {
            if (ModelState.IsValid)
            {
                db.Commentaries.Add(commentary);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(commentary);
        }

        public ActionResult Edit(int id)
        {
            var commentary = db.Commentaries.Find(id);
            ViewBag.IsInUse = IsCommentaryInUse(id);
            return View(commentary);
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
            var commentary = db.Commentaries.Find(id);
            db.Commentaries.Remove(commentary);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CommentsInFlightPhases()
        {
            var model = new CommentsInFlightPhasesViewModel();
            var phases = db.CommentaryTypes.OrderBy(x => x.DisplayOrder);
            var comments = db.Commentaries.OrderBy(x=>x.DisplayOrder);
            model.FlightPhaseNames = phases.Select(x => x.CType);
            model.Comments = new Dictionary<string, IEnumerable<CommentInFlightPhaseViewModel>>();
            foreach (var p in phases)
            {
                var cinp = comments.Select(c => new CommentInFlightPhaseViewModel
                {
                    Name = c.Comment,
                    DisplayOrder = 0,
                    UsedInPhase = c.CommentaryTypes.Select(x=>x.CType).ToList().Contains(p.CType)
                });
                model.Comments.Add(p.CType, cinp);
            }
            return View("CommentInFlightPhase", model);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        private bool IsCommentaryInUse(int commentaryId)
        {
            var inUse = db.TrainingFlightAnnotations.SelectMany(x => x.TrainingFlightAnnotationCommentCommentTypes)
                .Any(y => y.CommentaryId == commentaryId);

            return inUse;
        }

        public ActionResult SwapOrder(int itemId1, int itemId2)
        {
            var item1 = db.Commentaries.FirstOrDefault(x => x.CommentaryId== itemId1);
            var item2 = db.Commentaries.FirstOrDefault(x => x.CommentaryId == itemId2);
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
    }
}