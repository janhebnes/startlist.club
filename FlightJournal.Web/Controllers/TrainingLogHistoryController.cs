using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                var flightIds = db.AppliedExercises.Select(x=>x.FlightId).ToList();
                var flights = db.Flights.Where(x => x.Date.Year == year &&  flightIds.Contains(x.FlightId));
                model = CreateModel(flights);
                model.Year = year;
                model.Message = _("All training flights");
            }
            else if (Request.IsPilot())
            {
                // access to own flights (front or back)
                var pilotId = Request.Pilot().PilotId;
                var flightIds = db.AppliedExercises.Select(x => x.FlightId).ToList();
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

        public PartialViewResult GetDetails(string flightId)
        {
            if (Guid.TryParse(flightId, out var id))
            {
                var ae = db.AppliedExercises.Where(x => x.FlightId == id).Where(x=>x.Action != ExerciseAction.None);
                var annotation = db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == id);
                var weather = annotation?.Weather != null ? $"{annotation.Weather.WindDirection}­&deg; {annotation.Weather.WindSpeed.WindSpeedItem}kn " : "";
                var details = new TrainingFlightDetailsViewModel
                {
                    Exercises = ae.OrderBy(x => x.Lesson.DisplayOrder).ToList().Select(x => 
                        new Tuple<string, string, string, int, int>(x.Lesson.Name, x.Exercise.Name, x.Action.ToString(), x.Lesson.Training2LessonId, x.Exercise.Training2ExerciseId)),   //TODO: localize enum
                    Note = annotation?.Note ?? "",
                    Manouvres = new HtmlString(string.Join(", ", annotation?.Manouvres.Select(x => $"<i class='{x.IconCssClass}'></i>{x.ManouvreItem}") ?? Enumerable.Empty<string>())),
                    StartAnnotations = new HtmlString(string.Join(", ", annotation?.StartAnnotation.Select(x => $"{x.Comment}") ?? Enumerable.Empty<string>())),
                    FlightAnnotations = new HtmlString(string.Join(", ", annotation?.FlightAnnotation.Select(x => $"{x.Comment}") ?? Enumerable.Empty<string>())),
                    ApproachAnnotations = new HtmlString(string.Join(", ", annotation?.ApproachAnnotation.Select(x => $"{x.Comment}") ?? Enumerable.Empty<string>())),
                    LandingAnnotations = new HtmlString(string.Join(", ", annotation?.LandingAnnotation.Select(x => $"{x.Comment}") ?? Enumerable.Empty<string>())),
                    Weather = new HtmlString(weather)
                };
                return PartialView("_PartialTrainingFlightDetails", details);
            }
            return PartialView("_PartialTrainingFlightDetails", null);
        }

        private TrainingFlightHistoryViewModel CreateModel(IEnumerable<Flight> flights)
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
                    BackSeatOccupant = f.PilotBackseat != null  ? $"{f.PilotBackseat.Name} ({f.PilotBackseat.MemberId})" : "",
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString(),
                    TrainingProgramName = programName,
                    PrimaryLessonName = appliedLessons.FirstOrDefault().Key ?? "",
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryViewModel { Flights = flightModels, Message = flightModels.Any() ? "" : _("No flights")};
        }

        private string _(string resourceId)
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


    public class TrainingFlightHistoryViewModel
    {
        public IEnumerable<TrainingFlightViewModel> Flights { get; set; }
        public string  Message { get; set; }
        public int Year { get; set; }
    }

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

    public class TrainingFlightDetailsViewModel
    {
        public IEnumerable<Tuple<string,string,string, int, int>> Exercises { get; set; }
        public string Note { get; set; }
        public HtmlString Manouvres { get; set; }
        public HtmlString StartAnnotations { get; set; }
        public HtmlString FlightAnnotations { get; set; }
        public HtmlString ApproachAnnotations { get; set; }
        public HtmlString LandingAnnotations { get; set; }
        public HtmlString Weather { get; set; }
    }

}