using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor;
using CsvHelper;
using CsvHelper.Configuration;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Translations;

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

            TrainingFlightHistoryViewModel model;
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                // allow access to all flights, filters on front seat / back seat pilot
                var flightIds = db.AppliedExercises.Select(x=>x.FlightId).Union(db.TrainingFlightAnnotations.Select(y=>y.FlightId)).Distinct().ToList();
                var flights = db.Flights.Where(x => x.Date.Year == year &&  flightIds.Contains(x.FlightId));
                model = CreateModel(flights);
                model.Year = year;
                model.Message = _("All training flights");
            }
            else if (Request.IsPilot())
            {
                // access to own flights (front or back)
                var pilotId = Request.Pilot().PilotId;
                var flightIds = db.AppliedExercises.Select(x => x.FlightId).Union(db.TrainingFlightAnnotations.Select(y => y.FlightId)).Distinct().ToList();
                var flights = db.Flights.Where(x => x.Date.Year == year && flightIds.Contains(x.FlightId) && (x.PilotId == pilotId || x.PilotBackseatId == pilotId));

                model = CreateModel(flights);
                model.Year = year;
                model.Message = _("Your training flights");
            }
            else
            {
                // no access
                model = new TrainingFlightHistoryViewModel { Flights = Enumerable.Empty<TrainingFlightViewModel>(), Message = _("You do not have access to training flight logs")};
            }

            return View(model);
        }

        /// <summary>
        /// SHow flights for a pilot on a particular exercise
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

        public ActionResult ExportToCsv(int year = -1)
        {
            if (year == -1) year = DateTime.Now.Year;

            TrainingFlightHistoryExportViewModel viewModel;
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                // allow access to all flights, filters on front seat / back seat pilot
                var flightIds = db.AppliedExercises.Select(x => x.FlightId).Union(db.TrainingFlightAnnotations.Select(y => y.FlightId)).Distinct().ToList();
                var flights = db.Flights.Where(x => x.Date.Year == year && flightIds.Contains(x.FlightId));
                viewModel = CreateExportModel(db, flights, year);
            }
            else if (Request.IsPilot())
            {
                // access to own flights (front or back)
                var pilotId = Request.Pilot().PilotId;
                var flightIds = db.AppliedExercises.Select(x => x.FlightId).Union(db.TrainingFlightAnnotations.Select(y => y.FlightId)).Distinct().ToList();
                var flights = db.Flights.Where(x => x.Date.Year == year && flightIds.Contains(x.FlightId) && (x.PilotId == pilotId || x.PilotBackseatId == pilotId));

                viewModel = CreateExportModel(db, flights, year);
            }
            else
            {
                viewModel = new TrainingFlightHistoryExportViewModel();
            }

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
                var m = new TrainingFlightViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = (f.Landing ?? f.Date).ToString("yyyy-MM-dd HH:mm"),
                    Plane = $"{f.Plane.CompetitionId} ({f.Plane.Registration})",
                    FrontSeatOccupant = $"{f.Pilot.Name} ({f.Pilot.MemberId})",
                    BackSeatOccupant = f.PilotBackseat != null ? $"{f.PilotBackseat.Name} ({f.PilotBackseat.MemberId})" : "",
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

        private TrainingFlightHistoryExportViewModel CreateExportModel(FlightContext db, IEnumerable<Flight> flights, int year)
        {
            var flightModels = new List<TrainingFlightExportViewModel>();
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
                    PrimaryLessonName = primaryLessonName,
                    AppliedLessons = string.Join(", ", appliedLessons.OrderBy(x => x.Key.DisplayOrder).Select(x => x.Key.Name)),
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
        public int Year { get; set; } = -1;
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
        public string PrimaryLessonName { get; set; }
        public string AppliedLessons { get; set; }
    }
}