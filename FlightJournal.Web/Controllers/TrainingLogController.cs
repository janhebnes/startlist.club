using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
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


        public PartialViewResult GetRecentFlights(string flightId)
        {
            if (!Guid.TryParse(flightId, out var id)) return PartialView("_PartialTrainingLogView", null);

            var frontSeatPilotId = db.Flights.SingleOrDefault(x => x.FlightId == id)?.Pilot.PilotId;
            if(!frontSeatPilotId.HasValue) return PartialView("_PartialTrainingLogView", null);

            var flightsWithThisPilot = db.Flights.Where(x => x.Pilot.PilotId == frontSeatPilotId);
            var flightIdsWithThisPilot = flightsWithThisPilot.Select(f=>f.FlightId).ToList();

            var allTrainingFlightIdsForThisPilot = db.AppliedExercises
                .Select(x => x.FlightId)
                .Union(db.TrainingFlightAnnotations
                    .Select(y => y.FlightId))
                .Distinct()
                .Where(fid=>flightIdsWithThisPilot.Contains(fid))
                .ToList();
            var theFlights = flightsWithThisPilot.Where(f => allTrainingFlightIdsForThisPilot.Contains(f.FlightId)).OrderByDescending(x=>x.Landing ?? x.Date);

            var model = new List<TrainingFlightWithSomeDetailsViewModel>();


            foreach (var f in theFlights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Action != ExerciseAction.None);
                var programName = string.Join(", ", ae.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var appliedLessons = ae.Select(x => x.Lesson.Name).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value);
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);
                var weather = annotation?.Weather != null ? $"{annotation.Weather.WindDirection.WindDirectionItem}­&deg; {annotation.Weather.WindSpeed.WindSpeedItem}kn " : "";
                var commentsForPhasesInThisFlight = annotation
                                                        .TrainingFlightAnnotationCommentCommentTypes?
                                                        .GroupBy(e => e.CommentaryType.CType, e => e.Commentary, (phase, comments) => new { phase, comments })
                                                        .ToDictionary(
                                                            x => x.phase,
                                                            x => x.comments.Select(t => new HtmlString(t.Comment)))
                                                    ?? new Dictionary<string, IEnumerable<HtmlString>>();

                var m = new TrainingFlightWithSomeDetailsViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = f.Date.ToString("yyyy-MM-dd"),
                    Plane = $"{f.Plane.CompetitionId} ({f.Plane.Registration})",
                    FrontSeatOccupant = $"{f.Pilot.Name} ({f.Pilot.MemberId})",
                    BackSeatOccupant = f.PilotBackseat != null ? $"{f.PilotBackseat.Name} ({f.PilotBackseat.MemberId})" : "",
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    TrainingProgramName = programName,
                    PrimaryLessonName = appliedLessons.FirstOrDefault().Key ?? "",
                    Annotations = string.Join(", ",  commentsForPhasesInThisFlight.Select(x=>$"{x.Key}: {string.Join(",", x.Value)}")),
                    Manouvres = string.Join(", ", annotation?.Manouvres.Select(x => $"<i class='{x.IconCssClass}'></i>{new HtmlString(x.ManouvreItem)}") ?? Enumerable.Empty<string>()),
                    Note = annotation.Note
                };
                model.Add(m);
            }

            return PartialView("_PartialTrainingLogView", model);
        }

        private TrainingLogViewModel BuildTrainingLogViewModel(Flight flight, int trainingProgramId)
        {

            //var trainingProgress = db.TrainingFlightAnnotations.Join<Flight>;
            var pilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotId)?.Name ?? "(??)";
            var backseatPilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotBackseatId)?.Name ?? "(??)";

            var model = new TrainingLogViewModel(flight.FlightId, flight.Date, flight.Departure, flight.Landing, pilot, backseatPilot, new TrainingDataWrapper(db, flight.PilotId, flight, trainingProgramId));
            return model;
        }
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
        public PhaseComment[] phaseComments { get; set; }
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


    /// <summary>
    /// A specific training flight, overview
    /// </summary>
    public class TrainingFlightWithSomeDetailsViewModel
    {
        public string FlightId { get; set; }
        public string Timestamp { get; set; }
        public string Plane { get; set; }
        public string FrontSeatOccupant { get; set; }
        public string BackSeatOccupant { get; set; }
        public string Duration { get; set; }

        public string TrainingProgramName { get; set; }

        public string PrimaryLessonName { get; set; }
        public string Airfield { get; set; }
        public string Manouvres { get; set; }
        public string Annotations { get; set; }
        public string Note { get; set; }
    }

}