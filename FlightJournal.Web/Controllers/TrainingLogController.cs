using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Controllers
{
    public class TrainingLogController : Controller
    {
        private FlightContext db = new FlightContext();

        [Authorize]
        public ViewResult Edit(Guid flightId, int trainingProgramId = -1)
        {
            var flight = db.Flights.SingleOrDefault(x => x.FlightId == flightId);
            var model = BuildTrainingLogViewModel(flight, trainingProgramId);

            return View(model);
        }


        private TrainingLogViewModel BuildTrainingLogViewModel(Flight flight, int trainingProgramId)
        {

            //var trainingProgress = db.TrainingFlightAnnotations.Join<Flight>;
            var pilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotId)?.Name ?? "(??)";
            var backseatPilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotBackseatId)?.Name ?? "(??)";

            var model = new TrainingLogViewModel(flight.FlightId, flight.Date, flight.Departure, flight.Landing,  pilot, backseatPilot, new TrainingDataWrapper(db, flight.PilotId, flight,trainingProgramId));
            return model;
        }

        public ActionResult UpdateTrainingFlight(FlightData flightData)
        {
            //update data models based on flightData.
            if (!Guid.TryParse(flightData.flightId, out var flightId))
                return new JsonResult();

            // Upsert exercise for this flight. Note that flightData.exercises holds data for this flight only  (including any edits)
            var currentExercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId.Equals(flightId));
            // just retain latest edit
            db.AppliedExercises.RemoveRange(currentExercisesForThisFlight);
            if (flightData.exercises != null)
            {
                foreach (var e in flightData.exercises)
                {
                    var appliedExercise = new AppliedExercise
                    {
                        FlightId = flightId,
                        Program = db.TrainingPrograms.FirstOrDefault(p => p.Training2ProgramId == e.programId),
                        Lesson = db.TrainingLessons.FirstOrDefault(p => p.Training2LessonId == e.lessonId),
                        Exercise = db.TrainingExercises.FirstOrDefault(p => p.Training2ExerciseId == e.exerciseId),
                        Action = ExerciseAction.None
                    };

                    if (e.ok.HasValue && e.ok.Value)
                        appliedExercise.Action = ExerciseAction.Completed;
                    else if (e.trained.HasValue && e.trained.Value)
                        appliedExercise.Action = ExerciseAction.Trained;
                    else if (e.briefed.HasValue && e.briefed.Value)
                        appliedExercise.Action = ExerciseAction.Briefed;

                    db.AppliedExercises.AddOrUpdate(appliedExercise);
                }
            }

            // Upsert flight annotations for this flight
            var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId.Equals(flightId)) ?? new TrainingFlightAnnotation() {FlightId = flightId};
            
            annotation.Note = flightData.note;
            // ... flight phase comments
            annotation.TrainingFlightAnnotationCommentCommentTypes.Clear();
            if (flightData.phaseComments != null)
            {
                foreach (var item in flightData.phaseComments.Select(x => new TrainingFlightAnnotationCommentCommentType
                {
                    CommentaryTypeId = x.phaseId,
                    CommentaryId = x.commentId
                }))
                {
                    annotation.TrainingFlightAnnotationCommentCommentTypes.Add(item);
                }
            }

            // ... flight manouvres
            annotation.Manouvres.Clear();
            if (flightData.manouverIds != null)
            {
                foreach (var m in flightData.manouverIds
                    .Select(id => db.Manouvres.FirstOrDefault(m => m.ManouvreId == id)).Where(m => m != null))
                {
                    annotation.Manouvres.Add(m);
                }
            }

            // ... weather
            annotation.Weather = new Weather { WindDirection = new WindDirection { WindDirectionItem = flightData.wind_direction }, WindSpeed = new WindSpeed { WindSpeedItem = flightData.wind_speed }};

            //TODO weather seems a bit odd - could we simply just pass the numbers instead of representing them in db models ?

            db.TrainingFlightAnnotations.AddOrUpdate(annotation);

            db.SaveChanges();

            return new JsonResult();
        }

        public class FlightData
        {
            // ref to Flight
            public string flightId { get; set; }
            // exercise applied in this flight. Should go into AppliedExercises
            public Exercise[] exercises { get; set; }

            // the rest go into TrainingFlightAnnotations
            public int[] manouverIds { get; set; }
            public string note { get; set; }
            public PhaseComment[] phaseComments{ get; set; }
            public int wind_direction { get; set; }
            public int wind_speed { get; set; }
        }

        public class PhaseComment
        {
            public int phaseId { get; set; }
            public int commentId { get; set; }
        }
        public class Exercise
        {
            public int programId { get; set; }
            public int lessonId { get; set; }
            public int exerciseId { get; set; }
            public bool? briefed { get; set; }
            public bool? trained { get; set; }
            public bool? ok { get; set; }
        }

    }
}