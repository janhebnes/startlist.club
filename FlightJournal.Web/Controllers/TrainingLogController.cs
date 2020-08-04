using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Controllers
{
    public class TrainingLogController : Controller
    {
        private FlightContext db = new FlightContext();

        [Authorize]
        public ViewResult Edit(Guid flightId)
        {
            var flight = db.Flights.SingleOrDefault(x => x.FlightId == flightId);
            var model = BuildTrainingLogViewModel(flight);

            return View(model);
        }



        private TrainingLogViewModel BuildTrainingLogViewModel(Flight flight)
        {
            var pilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotId)?.Name ?? "(??)";
            var backseatPilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotBackseatId)?.Name ?? "(??)";

            var model = new TrainingLogViewModel(flight.Date, pilot, backseatPilot, new TrainingDataWrapper(db, flight.PilotId, flight));

            //var programs = new List<TrainingProgramViewModel>();
            //programs.Add(TrainingLogDemo.BuildSplWinchTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildSplTowTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildSplTmgTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildStartMethodWinchTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildStartMethodTowTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildTypeRatingSingleTrainingProgram());
            //programs.Add(TrainingLogDemo.BuildTypeRatingDualTrainingProgram());

            //model.TrainingPrograms = programs;

            return model;
        }

    }
}