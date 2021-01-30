using System.Data.Entity;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TrainingExerciseAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        public ActionResult Index(int trainingProgramId, int trainingLessonId)
        {
            FillViewBag(trainingProgramId,trainingLessonId);

            var lesson = db.TrainingLessons.Find(trainingLessonId);
            return View(lesson);
        }

        public ViewResult Details(int trainingProgramId, int trainingLessonId, int id)
        {
            FillViewBag(trainingProgramId, trainingLessonId);
            var exercise  = db.TrainingExercises.Find(id);
            return View(exercise);
        }

        public ActionResult Create(int trainingProgramId, int trainingLessonId)
        {
            FillViewBag(trainingProgramId, trainingLessonId);
            var exercise = new Training2Exercise();
            return View(exercise);
        }
        [HttpPost]
        public ActionResult Create(Training2Exercise exercise)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);
            int.TryParse(Request.Form.Get("tli"), out var tli);
            var program = db.TrainingPrograms.Find(tpi);
            var lesson = db.TrainingLessons.Find(tli);

            if (ModelState.IsValid)
            {
                exercise.Lessons.Add(lesson);
                db.TrainingExercises.Add(exercise);
                db.SaveChanges();
                return RedirectToAction("Index", new {trainingProgramId = tpi, trainingLessonId=tli });
            }

            FillViewBag(tpi,tli);
            return View(lesson);
        }

        public ActionResult Edit(int trainingProgramId, int trainingLessonId, int id)
        {
            FillViewBag(trainingProgramId, trainingLessonId);
            var exercise = db.TrainingExercises.Find(id);
            return View(exercise);
        }

        [HttpPost]
        public ActionResult Edit(Training2Exercise exercise)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);
            int.TryParse(Request.Form.Get("tli"), out var tli);

            if (ModelState.IsValid)
            {
                db.Entry(exercise).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { trainingProgramId = tpi, trainingLessonId = tli });
            }
            FillViewBag(tpi, tli);
            return View(exercise);
        }


        public ActionResult Delete(int trainingProgramId, int trainingLessonId, int id)
        {
            FillViewBag(trainingProgramId, trainingLessonId);
            var exercise = db.TrainingExercises.Find(id);

            return View(exercise);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            int.TryParse(Request.Form.Get("tpi"), out var tpi);
            int.TryParse(Request.Form.Get("tli"), out var tli);

            var exercise = db.TrainingExercises.Find(id);
            db.TrainingExercises.Remove(exercise);
            db.SaveChanges();

            return RedirectToAction("Index", new { trainingProgramId = tpi, trainingLessonId = tli });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void FillViewBag(int trainingProgramId, int trainingLessonId)
        {
            var program = db.TrainingPrograms.Find(trainingProgramId);
            var lesson = db.TrainingLessons.Find(trainingLessonId);
            ViewBag.TrainingProgramId = trainingProgramId;
            ViewBag.TrainingLessonId = trainingLessonId;
            ViewBag.TrainingProgramName = program.ShortName;
            ViewBag.TrainingLessonName = lesson.Name;
        }
    }
}