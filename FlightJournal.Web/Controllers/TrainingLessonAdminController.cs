using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TrainingLessonAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        public ActionResult Index(int trainingProgramId)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);

            if (program.Lessons.Count(x => x.DisplayOrder==0) > 1)
            {
                // not yet defined, set order according to current implicit order. You could argue that this should be done elsewhere...
                var order = 0;
                foreach (var lesson in program.Lessons)
                {
                    lesson.DisplayOrder = order++;
                    db.Entry(lesson).State = EntityState.Modified;
                }

                db.SaveChanges();
            }

            return View(program);
        }

        public ActionResult SwapLessonOrder(int trainingProgramId, int lessonId1, int lessonId2)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);

            var lesson1 = program.Lessons.FirstOrDefault(x => x.Training2LessonId == lessonId1);
            var lesson2 = program.Lessons.FirstOrDefault(x => x.Training2LessonId == lessonId2);
            if (lesson1 != null && lesson2 != null)
            {
                var tmp = lesson1.DisplayOrder;
                lesson1.DisplayOrder = lesson2.DisplayOrder;
                lesson2.DisplayOrder = tmp;
                db.Entry(lesson1).State = EntityState.Modified;
                db.Entry(lesson2).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { trainingProgramId });
        }

        public ViewResult Details(int trainingProgramId, int id)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);
            var lesson  = db.TrainingLessons.Find(id);
            ViewBag.TrainingProgramId = trainingProgramId;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";
            return View(lesson);
        }

        public ActionResult Create(int trainingProgramId)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);
            var lesson = new Training2Lesson();
            ViewBag.TrainingProgramId = trainingProgramId;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";
            return View(lesson);
        }
        [HttpPost]
        public ActionResult Create(Training2Lesson lesson)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);
            var program = db.TrainingPrograms.Find(tpi);

            if (ModelState.IsValid)
            {
                var lastOrder = program?.Lessons.Select(e => e.DisplayOrder).Max();
                if (lastOrder.HasValue)
                    lesson.DisplayOrder = lastOrder.Value + 1;

                lesson.Programs.Add(program); // backwards ?
                db.TrainingLessons.Add(lesson);
                db.SaveChanges();
                return RedirectToAction("Index", new {trainingProgramId=tpi });
            }

            ViewBag.TrainingProgramId = tpi;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";
            return View(lesson);
        }

        public ActionResult Edit(int trainingProgramId, int id)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);
            var lesson = db.TrainingLessons.Find(id);
            ViewBag.TrainingProgramId = trainingProgramId;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";
            return View(lesson);
        }

        [HttpPost]
        public ActionResult Edit(Training2Lesson lesson)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);
            var program = db.TrainingPrograms.Find(tpi);

            if (ModelState.IsValid)
            {
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", new { trainingProgramId = tpi });
            }
            ViewBag.TrainingProgramId = tpi;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";
            return View(lesson);
        }


        public ActionResult Delete(int trainingProgramId, int id)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);
            var lesson = db.TrainingLessons.Find(id);

            ViewBag.TrainingProgramId = trainingProgramId;
            ViewBag.TrainingProgramName = $"{program.ShortName} - {program.Name}";

            return View(lesson);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);

            var item = db.TrainingLessons.Find(id);
            db.TrainingLessons.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index", new { trainingProgramId = tpi });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}