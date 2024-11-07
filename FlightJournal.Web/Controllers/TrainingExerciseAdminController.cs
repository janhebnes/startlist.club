using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Controllers
{
    [NoCache]
    [Authorize(Roles = "Administrator")]
    public class TrainingExerciseAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        public ActionResult Index(int trainingProgramId, int trainingLessonId)
        {
            FillViewBag(trainingProgramId,trainingLessonId);

            var lesson = db.TrainingLessons.Find(trainingLessonId);

            if (lesson.Exercises.Count(x => x.DisplayOrder == 0) > 1)
            {
                // not yet defined, set order according to current implicit order. You could argue that this should be done elsewhere...
                var order = 0;
                foreach (var exercise in lesson.Exercises)
                {
                    exercise.DisplayOrder = order++;
                    db.Entry(exercise).State = EntityState.Modified;
                }

                db.SaveChanges();
            }

            ViewBag.CanDelete = lesson.Exercises.ToDictionary(x => x.Training2ExerciseId, x => !IsInUse(x.Training2ExerciseId));

            return View(lesson);
        }


        public ActionResult SwapExerciseOrder(int trainingProgramId, int trainingLessonId, int exerciseId1, int exerciseId2)
        {
            var lesson = db.TrainingLessons.Find(trainingLessonId);

            var exercise1 = lesson.Exercises.FirstOrDefault(x => x.Training2ExerciseId == exerciseId1);
            var exercise2 = lesson.Exercises.FirstOrDefault(x => x.Training2ExerciseId == exerciseId2);
            if (exercise1!= null && exercise2 != null)
            {
                var tmp = exercise1.DisplayOrder;
                exercise1.DisplayOrder = exercise2.DisplayOrder;
                exercise2.DisplayOrder = tmp;
                db.Entry(exercise1).State = EntityState.Modified;
                db.Entry(exercise2).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { trainingProgramId, trainingLessonId});
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
                var lastOrder = lesson?.Exercises?.Select(e => e.DisplayOrder).MaxOrDefault(0);
                if (lastOrder.HasValue)
                    exercise.DisplayOrder = lastOrder.Value + 1;

                exercise.Lessons.Add(lesson); // backwards ?
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
            ViewBag.IsInUse = IsInUse(id);
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



        private bool IsInUse(int id)
        {
            var isInUse = db.AppliedExercises.Any(x => x.Exercise.Training2ExerciseId == id);
            return isInUse;
        }
    }
}