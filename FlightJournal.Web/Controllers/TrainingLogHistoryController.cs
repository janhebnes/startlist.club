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
using FlightJournal.Web.Models.Training;
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
    [Authorize]
    public class TrainingLogHistoryController : Controller
    {
        /// <summary>Longest date range served in one request; longer ranges are clamped.</summary>
        private const int MaxRangeDays = 366;

        private readonly FlightContext db = new ();
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            // Access is decided before any training data is queried
            var scope = UsersAccessScope();

            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var now = DateTime.Now.Date;
                fromDate = now.AddDays(-now.Day + 1); // this month
                toDate = now;
            }
            var clamped = ClampRange(fromDate.Value, toDate.Value, out var from);

            TrainingFlightHistoryViewModel model;
            var currentClub = SelectedClub();
            switch (scope)
            {
                case AccessScope.AllFlights:
                    model = CreateModel(new TrainingHistoryFilter { From = from, To = toDate.Value, Club = currentClub }, currentClub);
                    model.Message = _("All training flights");
                    break;
                case AccessScope.OwnFlights:
                    model = CreateModel(new TrainingHistoryFilter { From = from, To = toDate.Value, PilotId = Request.Pilot().PilotId }, currentClub);
                    model.Message = _("Your training flights");
                    break;
                default:
                    model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>() };
                    model.Message = _("You do not have access to training flight logs");
                    break;
            }
            if (clamped)
                model.Message += " " + string.Format(_("The date range was limited to the last {0} days."), MaxRangeDays);
            model.FromDate = from;
            model.ToDate = toDate.Value;
            return View(model);
        }

        /// <summary>Limits a custom range to <see cref="MaxRangeDays"/> by moving its start; returns true when it did.</summary>
        private static bool ClampRange(DateTime fromDate, DateTime toDate, out DateTime clampedFrom)
        {
            clampedFrom = fromDate;
            if ((toDate - fromDate).TotalDays <= MaxRangeDays)
                return false;
            clampedFrom = toDate.AddDays(-MaxRangeDays);
            return true;
        }

        /// <summary>The selected club, captured once; null when no club is selected.</summary>
        private static ClubScope SelectedClub()
        {
            var club = ClubController.CurrentClub;
            return club.ShortName != null ? new ClubScope { ClubId = club.ClubId, LocationId = club.LocationId } : null;
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
                        model = CreateModel(new TrainingHistoryFilter { PilotId = pilotId, ProgramId = programId.Value }, SelectedClub());
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
                        model = CreateModel(new TrainingHistoryFilter { PilotId = pilotId, LessonId = lessonId.Value }, SelectedClub());
                        var template = _("Flights of pilot {0} on exercise {1}");
                        model.Message = string.Format(template, pilot.Name, lesson.Name);
                    }
                }
                else
                {
                    // everything
                    model = CreateModel(new TrainingHistoryFilter { PilotId = pilotId }, SelectedClub());
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


        /// <summary>Set-based selection for the exports (no id list); access is decided before the query is built.</summary>
        private IQueryable<Flight> SelectFlights(DateTime fromDate, DateTime toDate)
        {
            var query = new TrainingHistoryQuery(db);
            switch (UsersAccessScope())
            {
                case AccessScope.AllFlights:
                    return query.TrainingFlights(new TrainingHistoryFilter { From = fromDate, To = toDate, Club = SelectedClub() });
                case AccessScope.OwnFlights:
                    return query.TrainingFlights(new TrainingHistoryFilter { From = fromDate, To = toDate, PilotId = Request.Pilot().PilotId });
                default:
                    return Enumerable.Empty<Flight>().AsQueryable();
            }
        }


        /// <summary>Loads flat rows in a fixed number of queries and maps them to the (unchanged) view model.</summary>
        private TrainingFlightHistoryViewModel CreateModel(TrainingHistoryFilter filter, ClubScope currentClub)
        {
            var flightModels = new TrainingHistoryQuery(db).Load(filter, currentClub)
                .Select(r => new TrainingFlightViewModel
                {
                    FlightId = r.FlightId.ToString(),
                    IsCurrentClubPilots = r.IsCurrentClubPilots,
                    Timestamp = r.Timestamp,
                    Date = r.Date,
                    Plane = r.Plane,
                    FrontSeatOccupant = r.FrontSeatOccupant,
                    Instructor = r.Instructor,
                    Airfield = r.Airfield,
                    Duration = r.Duration,
                    LandingCount = r.LandingCount,
                    TrainingProgramName = r.TrainingProgramName,
                    PrimaryLessonName = r.PrimaryLessonName,
                    ExercisesFull = r.ExercisesFull,
                    ExercisesShort = r.ExercisesShort,
                    Manouvres = r.Manouvres,
                    Note = r.Note,
                    PassesValidation = r.PassesValidation,
                })
                .ToList();
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