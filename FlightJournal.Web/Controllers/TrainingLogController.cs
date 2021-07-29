using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Hubs;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Flight;

namespace FlightJournal.Web.Controllers
{
    public class TrainingLogController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        [Authorize]
        public ActionResult Edit(Guid? flightId, int trainingProgramId = -1)
        {
            if (!flightId.HasValue)
                return RedirectToAction("Grid", "Flight");
            var flight = db.Flights.SingleOrDefault(x => x.FlightId == flightId.Value);
            var model = BuildTrainingLogViewModel(flight, trainingProgramId);
            if (model.TrainingProgram.Id != -1)
                return View(model);
            else
                return View("SelectTrainingProgram", model);
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
            db.SaveChanges();

            var hasTrainingFlightData = false;

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
                        Instructor = db.Pilots.FirstOrDefault(p=>p.PilotId == flightData.instructorId)
                    };
                    if (e.gradingId.HasValue)
                    {
                        appliedExercise.Grading = db.Gradings.FirstOrDefault(x => x.GradingId == e.gradingId.Value);
                    }

                    db.AppliedExercises.AddOrUpdate(appliedExercise);
                    hasTrainingFlightData = true;
                }
            }
            db.SaveChanges();

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
            annotation.WindSpeed = flightData.wind_speed;
            annotation.WindDirection = flightData.wind_direction;

            db.TrainingFlightAnnotations.AddOrUpdate(annotation);

            db.SaveChanges();

            var flight = db.Flights.SingleOrDefault(f => f.FlightId == flightId);
            if (flight != null && hasTrainingFlightData != flight.HasTrainingData)
            {
                flight.HasTrainingData = hasTrainingFlightData;
                db.SaveChanges();
            }

            var originator = Guid.Empty;
            Guid.TryParse(flightData.originator, out originator);
            FlightsHub.NotifyTrainingDataChanged(flightId, originator);

            return new JsonResult() { @Data = new { Success = true } };
        }


        public PartialViewResult GetRecentFlights(string flightId)
        {
            if (!Guid.TryParse(flightId, out var id)) return PartialView("_PartialTrainingLogView", null);

            var frontSeatPilotId = db.Flights.SingleOrDefault(x => x.Deleted == null && x.FlightId == id)?.Pilot.PilotId;
            if(!frontSeatPilotId.HasValue) return PartialView("_PartialTrainingLogView", null);

            var flightsWithThisPilot = db.Flights.Where(x => x.Deleted == null && x.Pilot.PilotId == frontSeatPilotId);
            var flightIdsWithThisPilot = flightsWithThisPilot.Select(f=>f.FlightId).ToList();

            var allTrainingFlightIdsForThisPilot = db.AppliedExercises
                .Select(x => x.FlightId)
                .Distinct()
                .Where(fid=>flightIdsWithThisPilot.Contains(fid))
                .ToList();
            var theFlights = flightsWithThisPilot.Where(f => allTrainingFlightIdsForThisPilot.Contains(f.FlightId)).OrderByDescending(x=>x.Landing ?? x.Date);

            var model = new List<TrainingFlightWithSomeDetailsViewModel>();


            foreach (var f in theFlights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Grading != null && x.Grading.Value > 0);
                if (ae.IsNullOrEmpty())
                    continue;
                var programName = string.Join(", ", ae.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var appliedLessons = ae.Select(x => x.Lesson).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value).ToList();
                var primaryLessonName = "";
                if (!appliedLessons.IsNullOrEmpty())
                {
                    var primaryLesson = appliedLessons.Where(x => x.Value == appliedLessons.First().Value)
                        .OrderBy(x => x.Key.DisplayOrder).Last();
                    primaryLessonName = primaryLesson.Key.Name;
                }
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);
               // var weather = annotation?.Weather != null ? $"{annotation.Weather.WindDirection.WindDirectionItem}­&deg; {annotation.Weather.WindSpeed.WindSpeedItem}kn " : "";
                var phases = db.CommentaryTypes.OrderBy(x => x.DisplayOrder).ToList();
                var phaseComments = new Dictionary<string, IEnumerable<HtmlString>>();
                foreach (var p in phases)
                {
                    var annotationsForThisPhase = 
                        annotation?.TrainingFlightAnnotationCommentCommentTypes
                        .Where(x => x.CommentaryTypeId == p.CommentaryTypeId)
                        .Select(y => new HtmlString(y.Commentary.Comment)).ToList();
                    if(!annotationsForThisPhase.IsNullOrEmpty())
                        phaseComments.Add(p.CType, annotationsForThisPhase);   
                }

                var instructor = ae.FirstOrDefault(x => x.Instructor != null)?.Instructor;
                var instructorNameAndClub = instructor != null ? $"{instructor.Name} ({instructor.Club.ShortName})" : "";
                var m = new TrainingFlightWithSomeDetailsViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = (f.Landing ?? f.Date).ToString("yyyy-MM-dd HH:mm"),
                    Plane = $"{f.Plane.CompetitionId} ({f.Plane.Registration})",
                    FrontSeatOccupant = $"{f.Pilot.Name} ({f.Pilot.MemberId})",
                    BackSeatOccupant = f.PilotBackseat != null ? $"{f.PilotBackseat.Name} ({f.PilotBackseat.MemberId})" : "",
                    Instructor = instructorNameAndClub,
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    TrainingProgramName = programName,
                    PrimaryLessonName = primaryLessonName,
                    AppliedLessons = string.Join(", ", appliedLessons.OrderBy(x => x.Key.DisplayOrder).Select(x => x.Key.Name)),
                    Annotations = string.Join(", ", phaseComments.Select(x=>$"{x.Key}: {string.Join(",", x.Value)}")),
                    Manouvres = string.Join(", ", annotation?.Manouvres.Select(x => $"<i class='{x.IconCssClass}'></i>{new HtmlString(x.ManouvreItem)}") ?? Enumerable.Empty<string>()),
                    Note = annotation?.Note
                };
                model.Add(m);
            }

            return PartialView("_PartialTrainingLogView", model);
        }

        private TrainingLogViewModel BuildTrainingLogViewModel(Flight flight, int trainingProgramId)
        {

            //var trainingProgress = db.TrainingFlightAnnotations.Join<Flight>;
            var pilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotId)?.Name ?? "(??)";
            var backseatPilot = db.Pilots.SingleOrDefault(x => x.PilotId == flight.PilotBackseatId);
            var backseatPilotName = backseatPilot?.Name ?? "(??)";
            var instructorId = backseatPilot != null && backseatPilot.IsInstructor
                ? backseatPilot.PilotId
                : User.Pilot() != null && User.Pilot().IsInstructor
                    ? User.Pilot().PilotId
                    : -1;
            var model = new TrainingLogViewModel(flight.FlightId, flight.Date, flight.Departure, flight.Landing, pilot, backseatPilotName, new TrainingDataWrapper(db, flight.PilotId, instructorId, flight, trainingProgramId));
            return model;
        }
    }


    public class FlightData
    {
        public string originator { get; set; }
        // ref to Flight
        public string flightId { get; set; }
        public int instructorId { get; set; }
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
        public int? gradingId { get; set; }
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
        public string Instructor { get; set; }
        public string Duration { get; set; }

        public string TrainingProgramName { get; set; }

        public string PrimaryLessonName { get; set; }
        public string AppliedLessons { get; set; }
        public string Airfield { get; set; }
        public string Manouvres { get; set; }
        public string Annotations { get; set; }
        public string Note { get; set; }
    }

}