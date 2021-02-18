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

            var model = new TrainingLogViewModel(flight.FlightId, flight.Date, pilot, backseatPilot, new TrainingDataWrapper(db, flight.PilotId, flight,trainingProgramId));

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

        public ActionResult UpdateTrainingFlight(FlightData flightData)
        {
            //update data models based on flightData.
            if (!Guid.TryParse(flightData.flightId, out var flightId))
                return new JsonResult();

            // Upsert exercise for this flight. Note that flightData.exercises holds data for this flight only  (including any edits)
            var currentExercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId.Equals(flightId));
            foreach (var e in flightData.exercises)
            {
                var appliedExercise = currentExercisesForThisFlight.FirstOrDefault(x => x.AppliedExerciseId == e.exerciseId) ?? new AppliedExercise()
                {
                    FlightId = flightId,
                    Program = db.TrainingPrograms.FirstOrDefault(p=>p.Training2ProgramId == e.programId),
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

            // Upsert flight annotations for this flight
            var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId.Equals(flightId)) ?? new TrainingFlightAnnotation() {FlightId = flightId};
            
            //TODO: figure out why Commentary.AppliesToxxxx throws, and this does not. Seems to be the same code ?
            annotation.Note = flightData.note;
            var StartAnnotation = new List<Commentary>();
            var FlightAnnotation = new List<Commentary>();
            var ApproachAnnotation = new List<Commentary>();
            var LandingAnnotation = new List<Commentary>();
            StartAnnotation = flightData.s_annotationIds?.Select(id => db.Commentaries.FirstOrDefault(c => c.CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("start")) && c.CommentaryId == id)).Where(x => x != null).ToList();
            FlightAnnotation = flightData.f_annotationIds?.Select(id => db.Commentaries.FirstOrDefault(c => c.CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("flight")) && c.CommentaryId == id)).Where(x => x != null).ToList();
            ApproachAnnotation = flightData.i_annotationIds?.Select(id => db.Commentaries.FirstOrDefault(c => c.CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("approach")) && c.CommentaryId == id)).Where(x => x != null).ToList();
            LandingAnnotation = flightData.l_annotationIds?.Select(id => db.Commentaries.FirstOrDefault(c => c.CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("landing")) && c.CommentaryId == id)).Where(x=>x != null).ToList();
            annotation.Manouvres = flightData.manouverIds?.Select(id => db.Manouvres.FirstOrDefault(m => m.ManouvreId == id) ).Where(m => m != null).ToList();

            var commentcommentypeannotations = new List<TrainingFlightAnnotationCommentCommentType>();
            
            foreach (var s_annotation in StartAnnotation)
            {
                commentcommentypeannotations.Add(new TrainingFlightAnnotationCommentCommentType { TrainingFlightAnnotation = annotation, Commentary = s_annotation, CommentaryType = s_annotation.CommentaryTypes.Where(x=>x.CType.ToLower().Equals("start")).FirstOrDefault()});
            }
            foreach (var f_annotation in FlightAnnotation)
            {
                commentcommentypeannotations.Add(new TrainingFlightAnnotationCommentCommentType { TrainingFlightAnnotation = annotation, Commentary = f_annotation, CommentaryType = f_annotation.CommentaryTypes.Where(x => x.CType.ToLower().Equals("flight")).FirstOrDefault()});
            }
            foreach(var a_annotation in ApproachAnnotation)
            {
                commentcommentypeannotations.Add(new TrainingFlightAnnotationCommentCommentType { TrainingFlightAnnotation = annotation, Commentary = a_annotation, CommentaryType = a_annotation.CommentaryTypes.Where(x => x.CType.ToLower().Equals("approach")).FirstOrDefault()});
            }
            foreach (var l_annotation in LandingAnnotation)
            {
                commentcommentypeannotations.Add(new TrainingFlightAnnotationCommentCommentType { TrainingFlightAnnotation = annotation, Commentary = l_annotation, CommentaryType = l_annotation.CommentaryTypes.Where(x=>x.CType.ToLower().Equals("landing")).FirstOrDefault()});
            }

            annotation.TrainingFlightAnnotationCommentCommentTypes = commentcommentypeannotations;
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
            public int[] s_annotationIds{ get; set; }
            public int[] f_annotationIds{ get; set; }
            public int[] i_annotationIds{ get; set; }
            public int[] l_annotationIds{ get; set; }
            public int wind_direction { get; set; }
            public int wind_speed { get; set; }
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