using System.Collections.Generic;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using Flight = OGN.FlightLog.Client.Models.Flight;

namespace FlightJournal.Web.Controllers
{
    public class TrainingLogController : Controller
    {
        [Authorize]
        public ViewResult Edit(Flight id)
        {
            var model = BuildTrainingLogViewModel();

            return View(model);
        }



        private TrainingLogViewModel BuildTrainingLogViewModel()
        {
            //TODO: extract this from DB
            var model = new TrainingLogViewModel();

            var programs = new List<TrainingProgramViewModel>();


            programs.Add(TrainingLogDemo.BuildSplWinchTrainingProgram());
            programs.Add(TrainingLogDemo.BuildSplTowTrainingProgram());
            programs.Add(TrainingLogDemo.BuildSplTmgTrainingProgram());
            programs.Add(TrainingLogDemo.BuildStartMethodWinchTrainingProgram());
            programs.Add(TrainingLogDemo.BuildStartMethodTowTrainingProgram());
            programs.Add(TrainingLogDemo.BuildTypeRatingSingleTrainingProgram());
            programs.Add(TrainingLogDemo.BuildTypeRatingDualTrainingProgram());

            model.TrainingPrograms = programs;

            return model;
        }

    }
}