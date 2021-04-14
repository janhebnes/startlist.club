using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Export;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;
using FlightJournal.Web.Translations;
using Newtonsoft.Json;

namespace FlightJournal.Web.Controllers
{

    /// <summary>
    /// Handle history of training flights.
    ///
    /// Depending on user permissions, the data may cover multiple pilots.
    /// </summary>
    public class TrainingLogHistoryController : Controller
    {
        private readonly FlightContext db = new FlightContext();
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var now = DateTime.Now.Date;
                fromDate = now.AddDays(-now.Day + 1); // this month
                toDate = now;
            }

            var flights = SelectFlights(fromDate.Value, toDate.Value);
            var model = CreateModel(flights);
            model.FromDate = fromDate.Value;
            model.ToDate = toDate.Value;

            switch (UsersAccessScope())
            {
                case AccessScope.AllFlights:
                    model.Message = _("All training flights");
                    break;
                case AccessScope.OwnFlights:
                    model.Message = _("Your training flights");
                    break;
                default:
                    model.Message = _("You do not have access to training flight logs");
                    break;
            }
            return View(model);
        }


        /// <summary>
        /// Show flights for a pilot on a particular exercise
        /// </summary>
        /// <param name="pilotId"></param>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        public ActionResult PilotLessons(int pilotId, int lessonId)
        {
            TrainingFlightHistoryViewModel model;
            

            if (User.IsAdministrator() || Request.IsPilot() && (Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId))
            {
                var pilot = db.Pilots.SingleOrDefault(p => p.PilotId == pilotId);
                var lesson = db.TrainingLessons.SingleOrDefault(x => x.Training2LessonId == lessonId);
                if (pilot == null || lesson == null)
                {
                    model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("Unknown pilot or lesson id") };
                }
                else
                {
                    var flightIds = db.AppliedExercises.Where(x => x.Lesson.Training2LessonId == lessonId).Select(x => x.FlightId).Distinct().ToList();
                    var flights = db.Flights.Where(x => flightIds.Contains(x.FlightId) && (x.PilotId == pilotId || x.PilotBackseatId == pilotId));
                    model = CreateModel(flights);
                    var template = _("Flights of pilot {0} on exercise {1}");
                    model.Message = string.Format(template, pilot.Name, lesson.Name);
                }
                model.IsSinglePilot = true;
                model.IsSingleExercise = true;
                model.ShowButtonPanel = false;
            }
            else
            {
                // no access
                model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("You do not have access to training flight logs") };
            }

            return View("Index", model);
        }

        public PartialViewResult GetDetails(string flightId)
        {
            if (Guid.TryParse(flightId, out var id))
            {
                var details = CreateDetailsViewModel(id);
                return PartialView("_PartialTrainingFlightDetails", details);
            }
            return PartialView("_PartialTrainingFlightDetails", null);
        }

        public ActionResult ExportToCsv(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var now = DateTime.Now.Date;
                fromDate = now.AddDays(-now.Day + 1); // this month
                toDate = now;
            }

            var flights = SelectFlights(fromDate.Value, toDate.Value);
            var model = CreateExportModel(db, flights);

            var sb = new StringBuilder();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
            using (var writer = new StringWriter(sb))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(model.Flights);
            }
            
            return File(Encoding.UTF8.GetBytes(sb.ToString()), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{fromDate.Value.ToString("yyyyMMdd")}-{toDate.Value.ToString("yyyyMMdd")}.csv");
        }

        public ActionResult ExportToJson(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var now = DateTime.Now.Date;
                fromDate = now.AddDays(-now.Day + 1); // this month
                toDate = now;
            }

            var flights = SelectFlights(fromDate.Value, toDate.Value);
            var viewModel = CreateExportModel(db, flights);

            return File(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModel, Formatting.Indented)), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{fromDate.Value.ToString("yyyyMMdd")}-{toDate.Value.ToString("yyyyMMdd")}.json");

        }


        private IEnumerable<Flight> SelectFlights(DateTime fromDate, DateTime toDate)
        {
            IEnumerable<Flight> flights;
            var trainingFlightIds = db.AppliedExercises
                .Where(x=>x.Grading != null)
                .Select(x => x.FlightId)
                .Distinct().ToList();

            switch (UsersAccessScope())
            {
                case AccessScope.AllFlights:
                    flights = db.Flights.Where(x => x.Date >= fromDate && x.Date <= toDate && trainingFlightIds.Contains(x.FlightId));
                    if (ClubController.CurrentClub.ShortName != null)
                    {
                        flights = flights.Where(f =>
                            f.StartedFromId == ClubController.CurrentClub.LocationId
                            || f.LandedOnId == ClubController.CurrentClub.LocationId
                            || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                            || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                            || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId));
                    }
                    break;
                case AccessScope.OwnFlights:
                    var pilotId = Request.Pilot().PilotId;
                    flights = db.Flights.Where(x => x.Date >= fromDate && x.Date <= toDate && trainingFlightIds.Contains(x.FlightId) && (x.PilotId == pilotId || x.PilotBackseatId == pilotId));
                    break;
                default:
                    flights = Enumerable.Empty<Flight>();
                    break;
            }
            return flights;
        }


        private TrainingFlightHistoryViewModel CreateModel(IEnumerable<Flight> flights)
        {
            var flightModels = new List<TrainingFlightViewModel>();
            foreach (var f in flights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Grading != null && x.Grading.Value > 0);
                var programName = string.Join(", ", ae.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var appliedLessons = ae.Select(x => x.Lesson).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value).ToList();
                var primaryLessonName = "";
                if (!appliedLessons.IsNullOrEmpty())
                {
                    var primaryLesson = appliedLessons.Where(x => x.Value == appliedLessons.First().Value)
                        .OrderBy(x => x.Key.DisplayOrder).Last();
                    primaryLessonName = primaryLesson.Key.Name;
                }
                var instructor = ae.FirstOrDefault(x => x.Instructor != null)?.Instructor;
                var instructorNameAndClub = instructor != null ? $"{instructor.Name} ({instructor.Club.ShortName})" : "";

                var m = new TrainingFlightViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = (f.Landing ?? f.Date).ToString("yyyy-MM-dd HH:mm"),
                    Plane = $"{f.Plane.CompetitionId} ({f.Plane.Registration})",
                    FrontSeatOccupant = $"{f.Pilot.Name} ({f.Pilot.MemberId})",
                    Instructor = instructorNameAndClub,
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    TrainingProgramName = programName,
                    PrimaryLessonName = primaryLessonName,
                    AppliedLessons = string.Join(", ", appliedLessons.OrderBy(x=>x.Key.DisplayOrder).Select(x=>x.Key.Name))
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryViewModel { Flights = flightModels, Message = flightModels.Any() ? "" : _("No flights") };
        }

        private TrainingFlightDetailsViewModel CreateDetailsViewModel(Guid id)
        {
            var ae = db.AppliedExercises.Where(x => x.FlightId == id).Where(x => x.Grading != null && x.Grading.Value > 0);
            var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == id);
            var weather = annotation?.WindDirection != null && annotation?.WindSpeed != null ? $"{annotation.WindDirection}­&deg; {annotation.WindSpeed}kn " : "";
           
            var commentsForAllPhases = CommentsForFlight(annotation)
                .ToDictionary(
                    x=>x.Key.CType, 
                    x=>x.Value.Select(v => new HtmlString(v.Comment)));

            var details = new TrainingFlightDetailsViewModel
            {
                Exercises = ae.OrderBy(x => x.Lesson.DisplayOrder).ToList().Select(x =>
                    new TrainingFlightDetailsExerciseViewModel
                    {
                        LessonName = x.Lesson.Name,
                        ExerciseName = x.Exercise.Name,
                        GradingName = x.Grading?.Name,
                        LessonId = x.Lesson.Training2LessonId,
                        ExerciseId = x.Exercise.Training2ExerciseId
                    }),
                Note = annotation?.Note ?? "",
                Manouvres = new HtmlString(string.Join(", ", annotation?.Manouvres.Select(x => $"<i class='{x.IconCssClass}'></i>{x.ManouvreItem}") ?? Enumerable.Empty<string>())),
                FlightPhaseAnnotations = commentsForAllPhases,
                Weather = new HtmlString(weather)
            };
            return details;
        }



        private Dictionary<CommentaryType, IEnumerable<Commentary>> CommentsForFlight(TrainingFlightAnnotation annotation)
        {
            var commentsForPhasesInThisFlight = annotation?
                                                    .TrainingFlightAnnotationCommentCommentTypes?
                                                    .GroupBy(e => e.CommentaryType, e => e.Commentary, (phase, comments) => new { phase, comments })
                                                    .ToDictionary(
                                                        x => x.phase,
                                                        x => x.comments)
                                                ?? new Dictionary<CommentaryType, IEnumerable<Commentary>>();

            var commentsForAllPhases =
                db.CommentaryTypes
                    .OrderBy(c => c.DisplayOrder)
                    .ToDictionary(x => x, x => commentsForPhasesInThisFlight.GetOrDefault(x, Enumerable.Empty<Commentary>()));

            return commentsForAllPhases;
        }


        private TrainingFlightHistoryExportViewModel CreateExportModel(FlightContext db, IEnumerable<Flight> flights)
        {
            var flightModels = new List<TrainingFlightExportViewModel>();
            foreach (var f in flights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Grading != null && x.Grading.Value > 0).ToList();
                var program = ae.FirstOrDefault()?.Program;
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);

                var partialExercises = ae.Select(x => new TrainingFlightPartialExerciseExportViewModel(x)).ToList();
                var flightPhaseComments = CommentsForFlight(annotation)
                    .Where(x=>x.Value.Any())
                    .Select(x=>new CommentInFlightPhaseExportViewModel(x.Key, x.Value));
                var maneuvers = annotation != null
                    ? annotation.Manouvres?.Select(man => new ManeuverExportViewModel(man))
                    : Enumerable.Empty<ManeuverExportViewModel>();
                var instructor = ae.FirstOrDefault(x => x.Instructor != null)?.Instructor;
                var m = new TrainingFlightExportViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = f.Date.ToString("yyyy-MM-dd HH:mm"),
                    Registration = f.Plane.Registration,
                    Seats = f.Plane.Seats,
                    CompetitionId = f.Plane.CompetitionId,
                    FrontSeatOccupantName = f.Pilot.Name,
                    FrontSeatOccupantClubId = f.Pilot.MemberId,
                    FrontSeatOccupantUnionId = f.Pilot.UnionId,
                    BackSeatOccupantName = f.PilotBackseat?.Name,
                    BackSeatOccupantClubId = f.PilotBackseat?.MemberId,
                    BackSeatOccupantUnionId = f.PilotBackseat?.UnionId,
                    InstructorName = instructor?.Name,
                    InstructorClubId = instructor?.MemberId,
                    InstructorUnionId = instructor?.UnionId,
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    DurationInMinutes = Math.Round(f.Duration.TotalMinutes),
                    TrainingProgramName = program?.Name,
                    TrainingProgramId = program?.ProgramIdForExport.ToString(),
                    PartialExercises =  partialExercises,
                    FlightPhaseComments = flightPhaseComments,
                    Maneuvers = maneuvers,
                    Note = annotation?.Note
                };
            flightModels.Add(m);
            }
            return new TrainingFlightHistoryExportViewModel { Flights = flightModels, Timestamp = DateTimeOffset.Now, ExportingUser = User?.Identity?.Name };
        }

        private static string _(string resourceId)
        {
            return Internationalization.GetText(resourceId, Internationalization.LanguageCode);
        }



        public PartialViewResult ExerciseDetails(int trainingLessonId, int trainingExerciseid)
        {
            var lesson = db.TrainingLessons.Find(trainingLessonId);
            var exercise = db.TrainingExercises.Find(trainingExerciseid);
            return PartialView("_PartialExerciseDetailsView", new ExerciseDetailsViewModel(lesson, exercise));
        }



        private enum AccessScope
        {
            None,
            OwnFlights,
            AllFlights
        }

        private AccessScope UsersAccessScope()
        {
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
                return AccessScope.AllFlights;
            if (Request.IsPilot())
                return AccessScope.OwnFlights;
            return AccessScope.None;
        }


    }


    public class ExerciseDetailsViewModel
    {
        public ExerciseDetailsViewModel(Training2Lesson lesson, Training2Exercise exercise)
        {
            LessonName = lesson.Name;
            LessonDescription = lesson.Purpose;
            ExerciseName = exercise.Name;
            ExerciseDescription = exercise.Note;
        }

        public string LessonDescription { get; set; }

        public string ExerciseDescription { get; set; }

        public string ExerciseName { get; set; }

        public string LessonName { get; set; }
    }
    /// <summary>
    /// All training flights for a particular year
    /// </summary>
    public class TrainingFlightHistoryViewModel
    {
        public IEnumerable<TrainingFlightViewModel> Flights { get; set; }
        public string Message { get; set; }
        public bool IsSinglePilot { get; set; }
        public bool IsSingleExercise { get; set; }
        public bool ShowButtonPanel { get; set; } = true;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    /// <summary>
    /// A specific training flight, overview
    /// </summary>
    public class TrainingFlightViewModel
    {
        public string FlightId { get; set; }
        public string Timestamp { get; set; }
        public string Plane{ get; set; }
        public string FrontSeatOccupant { get; set; }
        public string Instructor { get; set; }
        public string Duration { get; set; }

        public string TrainingProgramName { get; set; }

        public string PrimaryLessonName { get; set; }
        public string AppliedLessons { get; set; }
        public string Airfield { get; set; }
    }

    /// <summary>
    /// A specific training flight, details
    /// </summary>
    public class TrainingFlightDetailsViewModel
    {
        public IEnumerable<TrainingFlightDetailsExerciseViewModel> Exercises { get; set; }
        public string Note { get; set; }
        public HtmlString Manouvres { get; set; }
        public Dictionary<string, IEnumerable<HtmlString>> FlightPhaseAnnotations { get; set; }
        public HtmlString Weather { get; set; }
    }

    /// <summary>
    /// A specific exercise in a specific training flight
    /// </summary>
    public class TrainingFlightDetailsExerciseViewModel
    {
        public string LessonName { get; set; }
        public string ExerciseName{ get; set; }
        public string GradingName { get; set; }
        public int LessonId { get; set; }
        public int ExerciseId { get; set; }
    }

}