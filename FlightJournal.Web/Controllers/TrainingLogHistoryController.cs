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
        private readonly FlightContext db = new ();
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var now = DateTime.Now.Date;
                fromDate = now.AddDays(-now.Day + 1); // this month
                toDate = now;
            }

            var flights = SelectFlights(fromDate.Value, toDate.Value);
            var model = CreateModel(flights.AsList());
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
        /// <param name="programId"></param>
        /// <returns></returns>
        public ActionResult PilotLessons(int pilotId, int? lessonId, int? programId)
        {
            TrainingFlightHistoryViewModel model;
            
            if (User.IsAdministrator() || Request.IsPilot() && (Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId))
            {
                var pilot = db.Pilots.SingleOrDefault(p => p.PilotId == pilotId);
                if (pilot == null)
                {
                    model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("Unknown pilot or lesson id") };
                }
                else if(programId.HasValue)
                {
                    var program = db.TrainingPrograms.SingleOrDefault(x => x.Training2ProgramId == programId.Value);
                    if(program==null)
                        model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("Unknown pilot or program id") };
                    else
                    {
                        var flightIds = DbHelper.IdsOfTrainingFlightsWithProgramId(db, programId.Value);
                        var flights = GetFlightsFromIdsAndMore(
                            flightIds,
                            pilotId,
                            null,
                            null);
                        model = CreateModel(flights.AsList());
                        var template = _("Flights of pilot {0} on program {1}");
                        model.Message = string.Format(template, pilot.Name, program.Name);
                    }
                }
                else if(lessonId.HasValue)
                {
                    var lesson = db.TrainingLessons.SingleOrDefault(x => x.Training2LessonId == lessonId.Value);
                    if(lesson==null)
                        model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("Unknown pilot or lesson id") };
                    else
                    {
                        var flightIds = DbHelper.IdsOfTrainingFlightsWithLesson(db, lessonId.Value);
                        var flights = GetFlightsFromIdsAndMore(
                            flightIds,
                            pilotId,
                            null,
                            null);
                        model = CreateModel(flights.AsList());
                        var template = _("Flights of pilot {0} on exercise {1}");
                        model.Message = string.Format(template, pilot.Name, lesson.Name);
                    }
                }
                else
                {
                    // everything
                    var flightIds = DbHelper.IdsOfTrainingFlights(db);
                    var flights = GetFlightsFromIdsAndMore(
                        flightIds,
                        pilotId,
                        null,
                        null);
                    model = CreateModel(flights.AsList());
                    var template = _("Flights of pilot {0}");
                    model.Message = string.Format(template, pilot.Name);
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

        private IEnumerable<Flight> GetFlightsFromIdsAndMore(HashSet<Guid> ids, int pilotId, DateTime? fromDate, DateTime? toDate)
        {
            var result =  db.Flights.Where(x => x.Deleted == null && ids.Contains(x.FlightId));
            if(pilotId > -1)
                result = result.Where(x=>x.PilotId == pilotId || x.PilotBackseatId == pilotId);
            if (fromDate.HasValue)
                result = result.Where(x => x.Date >= fromDate.Value);
            if (toDate.HasValue)
                result = result.Where(x => x.Date <= toDate.Value);

            return result;
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
            var model = CreateExportModel(db, flights, User?.Identity?.Name);

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
            
            return File(Encoding.UTF8.GetBytes(sb.ToString()), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{fromDate.Value:yyyyMMdd}-{toDate.Value:yyyyMMdd}.csv");
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
            var viewModel = CreateExportModel(db, flights, User?.Identity?.Name);

            return File(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModel, Formatting.Indented)), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{fromDate.Value:yyyyMMdd}-{toDate.Value:yyyyMMdd}.json");

        }


        private IEnumerable<Flight> SelectFlights(DateTime fromDate, DateTime toDate)
        {
            IEnumerable<Flight> flights;
            var trainingFlightIds = DbHelper.IdsOfTrainingFlights(db);

            switch (UsersAccessScope())
            {
                case AccessScope.AllFlights:
                    flights = GetFlightsFromIdsAndMore(trainingFlightIds, -1, fromDate, toDate);
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
                    flights = GetFlightsFromIdsAndMore(trainingFlightIds, pilotId, fromDate, toDate);
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
            var flightIds = flights.Select(x => x.FlightId).ToHashSet();
            var exercisesForTheseFlights = db.AppliedExercises
                    .Where(x => flightIds.Contains(x.FlightId))
                    .AsReadOnlyList();

            foreach (var f in flights)
            {
                var exercisesForThisFlight = exercisesForTheseFlights.Where(x => x.FlightId == f.FlightId).AsReadOnlyList();
                var programName = string.Join(", ", exercisesForThisFlight.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var lessonsForThisFlight = exercisesForThisFlight.Select(x => x.Lesson).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value).ToList();
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);
                var primaryLessonName = "";
                if (!lessonsForThisFlight.IsNullOrEmpty())
                {
                    var primaryLesson = lessonsForThisFlight.Where(x => x.Value == lessonsForThisFlight.First().Value)
                        .OrderBy(x => x.Key.DisplayOrder).Last();
                    primaryLessonName = primaryLesson.Key.Name;
                }
                var instructor = exercisesForThisFlight.FirstOrDefault(x => x.Instructor != null)?.Instructor;
                var instructorNameAndClub = instructor != null ? $"{instructor.Name} ({instructor.Club.ShortName})" : "";
                var validator = new TrainingFlightExportValidator(f, exercisesForThisFlight);
                var flightPhaseComments = CommentsForFlight(db, annotation)
                    .Where(x => x.Value.Any())
                    .Select(x => new CommentInFlightPhaseExportViewModel(x.Key, x.Value))
                    .Select(x => $"{x.FlightPhase.Phase}: {string.Join(",", x.Comments.Select(y => y.Comment))}");
                var allComments = new List<string> { annotation?.Note };
                allComments.AddRange(flightPhaseComments);
                var exercisesLong = exercisesForThisFlight
                    .OrderBy(x => x.Lesson.DisplayOrder)
                    .ThenBy(x => x.Exercise.DisplayOrder)
                    .Select(x => $"{x.Lesson.Name}-{x.Exercise.Name} ({x.Grading?.Value})").ToList();
                var exercisesShort = exercisesForThisFlight
                    .OrderBy(x => x.Lesson.DisplayOrder)
                    .Select(x => $"{x.Lesson.Name}")
                    .Distinct()
                    .ToList();

                var plane = f.Plane;
                var pilot = f.Pilot;
                var startedFrom = f.StartedFrom;
                var manouvres = annotation?.Manouvres;

                var m = new TrainingFlightViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    IsCurrentClubPilots = f.IsCurrentClubPilots(),
                    Timestamp = (f.Landing ?? f.Date).ToString("yyyy-MM-dd HH:mm"),
                    Date = f.Date,
                    Plane = $"{plane.CompetitionId} ({plane.Registration})",
                    FrontSeatOccupant = $"{pilot.Name} ({pilot.MemberId})",
                    Instructor = instructorNameAndClub,
                    Airfield = startedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    LandingCount = f.LandingCount,
                    TrainingProgramName = programName,
                    PrimaryLessonName = primaryLessonName,
                    ExercisesFull = string.Join(", ", exercisesLong),
                    ExercisesShort = string.Join(", ", exercisesShort),
                    Manouvres = string.Join(", ", manouvres?.Select(x => $"<i class='{x.IconCssClass}'></i>{new HtmlString(x.ManouvreItem)}") ?? Enumerable.Empty<string>()),
                    Note = string.Join(" - ", allComments.Where(x=>!x.IsNullOrEmpty())),
                    PassesValidation = validator.IsValid,
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryViewModel { Flights = flightModels, Message = flightModels.Any() ? "" : _("No flights") };
        }

        private TrainingFlightDetailsViewModel CreateDetailsViewModel(Guid flightId)
        {
            var flight = db.Flights.SingleOrDefault(x => x.FlightId == flightId);
            if (flight == null)
                return null;
            var timestamp = flight.Departure;
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flightId);
            var annotationForThisFlight = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == flightId);
            var weather = annotationForThisFlight != null ? $"{annotationForThisFlight.WindDirection}­&deg; {annotationForThisFlight.WindSpeed}kn " : "";
           
            var commentsForAllPhases = CommentsForFlight(db, annotationForThisFlight)
                .ToDictionary(
                    x=>x.Key.CType, 
                    x=>x.Value.Select(v => new HtmlString(v.Comment)));

            var validator = new TrainingFlightExportValidator(flight, exercisesForThisFlight);

            var details = new TrainingFlightDetailsViewModel
            {
                Exercises = exercisesForThisFlight.OrderBy(x => x.Lesson.DisplayOrder).ToList().Select(x =>
                    new TrainingFlightDetailsExerciseViewModel
                    {
                        LessonName = x.Lesson.Name,
                        ExerciseName = x.Exercise.Name,
                        GradingName = x.Grading?.Name,
                        GradingValue = x.Grading?.Value.ToString() ?? "",
                        LessonId = x.Lesson.Training2LessonId,
                        ExerciseId = x.Exercise.Training2ExerciseId,
                    }),
                Note = annotationForThisFlight?.Note ?? "",
                Manouvres = new HtmlString(string.Join(", ", annotationForThisFlight?.Manouvres.Select(x => $"<i class='{x.IconCssClass}'></i>{x.ManouvreItem}") ?? Enumerable.Empty<string>())),
                FlightPhaseAnnotations = commentsForAllPhases,
                Weather = new HtmlString(weather),
                PassesValidation = validator.IsValid,
                Violations = validator.Violations,
                Timestamp = timestamp.HasValue ? timestamp.Value.ToString("F") : ""
            };
            return details;
        }


        //static, used by other controllers
        private static Dictionary<CommentaryType, IEnumerable<Commentary>> CommentsForFlight(FlightContext db, TrainingFlightAnnotation annotation)
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


        //static, used by other controllers
        public static TrainingFlightHistoryExportViewModel CreateExportModel(FlightContext db, IEnumerable<Flight> flights, string exportingUser)
        {
            var flightModels = new List<TrainingFlightExportViewModel>();
            foreach (var f in flights)
            {
                var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).AsReadOnlyList();
                var program = exercisesForThisFlight.FirstOrDefault()?.Program;
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);

                var partialExercises = exercisesForThisFlight.Select(x => new TrainingFlightPartialExerciseExportViewModel(x)).ToList();
                var flightPhaseComments = CommentsForFlight(db, annotation)
                    .Where(x=>x.Value.Any())
                    .Select(x=>new CommentInFlightPhaseExportViewModel(x.Key, x.Value));
                var maneuvers = annotation != null
                    ? annotation.Manouvres?.Select(man => new ManeuverExportViewModel(man))
                    : Enumerable.Empty<ManeuverExportViewModel>();
                var instructor = exercisesForThisFlight.FirstOrDefault(x => x.Instructor != null)?.Instructor;
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
                    Note = annotation?.Note,
                    LandingCount = f.LandingCount
                };
                m.TrimAll();
            flightModels.Add(m);
            }
            return new TrainingFlightHistoryExportViewModel { Flights = flightModels, Timestamp = DateTimeOffset.Now, ExportingUser = exportingUser};
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
        public string ExercisesFull { get; set; }
        public string ExercisesShort { get; set; }
        public string Manouvres { get; set; }
        public string Airfield { get; set; }

        public bool PassesValidation { get; set; }
        public bool IsCurrentClubPilots { get; set; }
        public DateTime Date { get; set; }
        public int LandingCount { get; set; }
        public string Note { get; set; }
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
        public bool PassesValidation { get; set; }
        public IEnumerable<string> Violations { get; set; }
        public string Timestamp { get; set; }
    }

    /// <summary>
    /// A specific exercise in a specific training flight
    /// </summary>
    public class TrainingFlightDetailsExerciseViewModel
    {
        public string LessonName { get; set; }
        public string ExerciseName{ get; set; }
        public string GradingName { get; set; }
        public string GradingValue { get; set; }
        public int LessonId { get; set; }
        public int ExerciseId { get; set; }
    }

}