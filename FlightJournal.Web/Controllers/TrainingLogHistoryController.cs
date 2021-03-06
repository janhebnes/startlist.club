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
using CsvHelper.Configuration.Attributes;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
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
        public ActionResult Index(int year=-1)
        {
            if (year == -1) year = DateTime.Now.Year;

            var flights = SelectFlightsForExport(year);
            var model = CreateModel(flights);
            model.Year = year;
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

        public PartialViewResult GetDetails(string flightId)
        {
            if (Guid.TryParse(flightId, out var id))
            {
                var details = CreateDetailsViewModel(id);
                return PartialView("_PartialTrainingFlightDetails", details);
            }
            return PartialView("_PartialTrainingFlightDetails", null);
        }

        public ActionResult ExportToCsv(int year = -1)
        {
            if (year == -1) year = DateTime.Now.Year;

            var flights = SelectFlightsForExport(year);
            var viewModel = CreateExportModel(db, flights, year);

            var sb = new StringBuilder();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
            using (var writer = new StringWriter(sb))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(viewModel.Flights);
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{year}.csv");
        }

        public ActionResult ExportToJson(int year = -1)
        {
            if (year == -1) year = DateTime.Now.Year;

            var flights = SelectFlightsForExport(year);
            var viewModel = CreateExportModel(db, flights, year);
            
            return File(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModel, Formatting.Indented)), System.Net.Mime.MediaTypeNames.Application.Octet, $"TrainingFlights-{year}.json");

        }
        public TrainingFlightHistoryViewModel CreateModel(IEnumerable<Flight> flights)
        {
            var flightModels = new List<TrainingFlightViewModel>();
            foreach (var f in flights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Action != ExerciseAction.None);
                var programName = string.Join(", ", ae.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var appliedLessons = ae.Select(x => x.Lesson.Name).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value);
                var m = new TrainingFlightViewModel
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
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryViewModel { Flights = flightModels, Message = flightModels.Any() ? "" : _("No flights") };
        }

        public TrainingFlightDetailsViewModel CreateDetailsViewModel(Guid id)
        {
            var ae = db.AppliedExercises.Where(x => x.FlightId == id).Where(x => x.Action != ExerciseAction.None);
            var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == id);
            var weather = annotation?.WindDirection != null && annotation?.WindSpeed != null ? $"{annotation.WindDirection}­&deg; {annotation.WindSpeed}kn " : "";
           
            var commentsForPhasesInThisFlight = annotation
                .TrainingFlightAnnotationCommentCommentTypes?
                .GroupBy(e => e.CommentaryType.CType, e => e.Commentary, (phase, comments) => new { phase, comments })
                .ToDictionary(
                    x => x.phase,
                    x => x.comments.Select(t => new HtmlString(t.Comment)))
                                                ?? new Dictionary<string, IEnumerable<HtmlString>>();

            var commentsForAllPhases =
                db.CommentaryTypes
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => c.CType)
                    .ToDictionary(x => x, x => commentsForPhasesInThisFlight.GetOrDefault(x, Enumerable.Empty<HtmlString>()));

            var details = new TrainingFlightDetailsViewModel
            {
                Exercises = ae.OrderBy(x => x.Lesson.DisplayOrder).ToList().Select(x =>
                    new TrainingFlightDetailsExerciseViewModel
                    {
                        LessonName = x.Lesson.Name,
                        ExerciseName = x.Exercise.Name,
                        ActionName = x.Action.ToString(),  //TODO: localize enum
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

        private IEnumerable<Flight> SelectFlightsForExport(int year)
        {
            IEnumerable<Flight> flights;
            var flightIds = db.AppliedExercises.Select(x => x.FlightId).Union(db.TrainingFlightAnnotations.Select(y => y.FlightId)).Distinct().ToList();
            switch (UsersAccessScope())
            {
                case AccessScope.AllFlights:
                    flights = db.Flights.Where(x => x.Date.Year == year && flightIds.Contains(x.FlightId));
                    break;
                case AccessScope.OwnFlights:
                    var pilotId = Request.Pilot().PilotId;
                    flights = db.Flights.Where(x => x.Date.Year == year && flightIds.Contains(x.FlightId) && (x.PilotId == pilotId || x.PilotBackseatId == pilotId));
                    break;
                default:
                    flights = Enumerable.Empty<Flight>();
                    break;
            }
            return flights;
        }

        private TrainingFlightHistoryExportViewModel CreateExportModel(FlightContext db, IEnumerable<Flight> flights, int year)
        {
            var flightModels = new List<TrainingFlightExportViewModel>();
            foreach (var f in flights)
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Action != ExerciseAction.None);
                var tfa = db.TrainingFlightAnnotations.SingleOrDefault(x => x.FlightId == f.FlightId);
                var programName = string.Join(", ", ae.Select(x => x.Program.ShortName).Distinct()); // should be only one on a single flight, but...
                var appliedLessons = ae.Select(x => x.Lesson.Name).GroupBy(a => a).ToDictionary((g) => g.Key, g => g.Count()).OrderByDescending(d => d.Value);
                var phaseComments = new List<GradedItemViewModel>();
                if (tfa != null)
                {
                    var phases = db.CommentaryTypes.OrderBy(x => x.DisplayOrder).ToList();
                    foreach (var p in phases)
                    {
                        var annotationsForThisPhase =
                            tfa.TrainingFlightAnnotationCommentCommentTypes
                                .Where(x => x.CommentaryTypeId == p.CommentaryTypeId)
                                .Select(y => y.Commentary.Comment).ToList();
                        if (!annotationsForThisPhase.IsNullOrEmpty())
                            phaseComments.Add(new GradedItemViewModel{Name=p.CType, Grading = string.Join(", ", annotationsForThisPhase)});
                    }

                }
                var m = new TrainingFlightExportViewModel
                {
                    Timestamp = f.Date.ToString("yyyy-MM-dd"),
                    Registration = f.Plane.Registration,
                    CompetitionId = f.Plane.CompetitionId,
                    FrontSeatOccupantName = f.Pilot.Name,
                    FrontSeatOccupantClubId = f.Pilot.MemberId,
                    FrontSeatOccupantUnionId = f.Pilot.UnionId,
                    FrontSeatOccupantInstructorId = f.Pilot.InstructorId,
                    BackSeatOccupantName = f.PilotBackseat?.Name,
                    BackSeatOccupantClubId = f.PilotBackseat?.MemberId,
                    BackSeatOccupantUnionId = f.PilotBackseat?.UnionId,
                    BackSeatOccupantInstructorId = f.PilotBackseat?.InstructorId,
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    DurationInMinutes = f.Duration.TotalMinutes,
                    TrainingProgramName = programName,
                    PrimaryExerciseName = appliedLessons.FirstOrDefault().Key ?? "",
                    PartialExerciseGradings = ae.Select(x=>new GradedItemViewModel{Name = x.Exercise.Name, Grading = x.Action.ToString()}).ToList(),
                    FlightPhaseComments = phaseComments,
                    Note = tfa?.Note,
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryExportViewModel { Flights = flightModels};
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
        public string  Message { get; set; }
        public int Year { get; set; }
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
        public string BackSeatOccupant { get; set; }
        public string Duration { get; set; }

        public string TrainingProgramName { get; set; }

        public string PrimaryLessonName { get; set; }
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
        public string ActionName { get; set; }
        public int LessonId { get; set; }
        public int ExerciseId { get; set; }
    }

    /// <summary>
    /// Export of all flights (used for a specific year)
    /// </summary>
    internal class TrainingFlightHistoryExportViewModel
    {
        public List<TrainingFlightExportViewModel> Flights { get; set; } = new List<TrainingFlightExportViewModel>();
    }

    /// <summary>
    /// Export of a specific flight
    /// </summary>
    internal class TrainingFlightExportViewModel
    {
        public string Timestamp { get; set; }
        public string Registration { get; set; }
        public string CompetitionId { get; set; }
        public string FrontSeatOccupantName { get; set; }
        public string FrontSeatOccupantClubId { get; set; }
        public string FrontSeatOccupantUnionId { get; set; }
        public string FrontSeatOccupantInstructorId { get; set; }
        public string BackSeatOccupantName { get; set; }
        public string BackSeatOccupantClubId { get; set; }
        public string BackSeatOccupantUnionId { get; set; }
        public string BackSeatOccupantInstructorId { get; set; }
        public string Airfield { get; set; }
        public string Duration { get; set; }
        public double DurationInMinutes { get; set; }
        public string TrainingProgramName { get; set; }
        public string PrimaryExerciseName { get; set; }

        [Ignore]
        public IEnumerable<GradedItemViewModel> PartialExerciseGradings { get; set; }
        [Ignore]
        public IEnumerable<GradedItemViewModel> FlightPhaseComments { get; set; }

        [JsonIgnore]
        public string FlattenedPartialExerciseGradings => string.Join("|", PartialExerciseGradings?.Select(x => $"{x.Name}:{x.Grading}"));
        [JsonIgnore]
        public string FlattenedFlightPhaseComments => string.Join("|", FlightPhaseComments?.Select(x => $"{x.Name}:{x.Grading}"));
        public string Note { get; set; }
    }

    internal class GradedItemViewModel
    {
        public string Name { get; set; }
        public string Grading { get; set; }
    }
}