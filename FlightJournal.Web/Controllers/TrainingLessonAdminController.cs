using System.Data.Entity;
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

            return View(program);
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
                lesson.Programs.Add(program);
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