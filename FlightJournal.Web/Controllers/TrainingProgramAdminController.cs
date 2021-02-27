using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using Newtonsoft.Json;

namespace FlightJournal.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TrainingProgramAdminController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        public ActionResult Index(int dtoId=-1)
        {
            var items = db.TrainingPrograms;
            ViewBag.CanDelete = items.ToDictionary(x => x.Training2ProgramId, x => !IsInUse(x.Training2ProgramId));

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
            ViewBag.IsInUse = IsInUse(id);
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



        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    using(var sr = new StreamReader(file.InputStream))
                    using (var reader = new JsonTextReader(sr))
                    {
                        var program = JsonConvert.DeserializeObject<Training2Program>(sr.ReadToEnd());
                        db.TrainingPrograms.Add(program);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception _)
            {
            }
            return RedirectToAction("Index");
        }

        private bool IsInUse(int id)
        {
            var isInUse = db.AppliedExercises.Any(x => x.Program.Training2ProgramId == id);
            return isInUse;
        }
    }
}