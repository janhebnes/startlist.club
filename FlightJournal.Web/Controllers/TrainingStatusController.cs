using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;
using Microsoft.Ajax.Utilities;
using Microsoft.Owin.Security;

namespace FlightJournal.Web.Controllers
{
    internal class CachedTrainingProgram
    {
        public int Training2ProgramId;

        public List<CachedTrainingLesson> Lessons;
    }

    internal class CachedTrainingLesson
    {
        public int TrainingLessonId;
        public List<CachedTrainingExercise> Exercises;
    }
    internal class CachedTrainingExercise
    {
        public int TrainingExerciseId;

    }


    public class TrainingStatusController : Controller
    {
        private readonly FlightContext db = new FlightContext();
        private List<Training2Exercise> trainingExercises = new FlightContext().TrainingExercises.ToList();
        private List<Training2Lesson> trainingLessons = new FlightContext().TrainingLessons.ToList();
        private readonly List<Training2Program> trainingPrograms = new FlightContext().TrainingPrograms.ToList();
        private readonly List<Pilot> pilots= new FlightContext().Pilots.ToList();
        private readonly Club CurrentClub = ClubController.CurrentClub;

        private readonly DateTime FirstRelevantDate = DateTime.Now.AddYears(-3); // flights before this are hardly relevant

        private List<string> developerInfo = new List<string>();

        // GET: TrainingStatus
        public ActionResult Index()
        {
            var sw = Stopwatch.StartNew();
            var model = new List<TrainingProgramStatus>();
            // pay up front
            trainingLessons.ForEach(x=>x.Exercises = trainingExercises.Where(y=>y.Lessons.Select(z=>z.Training2LessonId).Contains(x.Training2LessonId)).ToList());
            trainingPrograms.ForEach(x=>x.Lessons= trainingLessons.Where(y=>y.Programs.Select(z=>z.Training2ProgramId).Contains(x.Training2ProgramId)).ToList());

            var allFlownExercises = db.AppliedExercises.Where(x => x.Grading != null).ToList();
            var allTrainingFlights = GetTrainingFlightsFromIds(allFlownExercises.Select(x => x.FlightId).Distinct().ToList());
            Trace.WriteLine($"## got {allTrainingFlights.Count} flights and {allFlownExercises.Count} exercises in {sw.Elapsed}");

            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                List<Pilot> flyingPilots;
                if (CurrentClub.ShortName == null)
                {
                    flyingPilots = allTrainingFlights.Select(f => f.Pilot)
                        .OrderBy(p => p.Name)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    flyingPilots = allTrainingFlights.Select(f => f.Pilot)
                        .Where(p => p.ClubId == CurrentClub.ClubId)
                        .OrderBy(p => p.Name)
                        .Distinct()
                        .ToList();
                }
                Trace.WriteLine($"## Got {flyingPilots.Count()} pilots in {sw.Elapsed}");
                foreach ( var p in flyingPilots)
                {
                    model.AddRange(GetStatusForPilot(p, allTrainingFlights, allFlownExercises));
                }
                Trace.WriteLine($"## Got all status in {sw.Elapsed} ({sw.Elapsed.TotalSeconds/flyingPilots.Count()} s/pilot)");

            }
            else if (Request.IsPilot())
            {
                model.AddRange(GetStatusForPilot(Request.Pilot(), allTrainingFlights, allFlownExercises));
            }
            else
            {
                // no access
            }

            ViewBag.DeveloperInfo = developerInfo;
            return View(model);
        }


        public ActionResult PilotStatusDetails(int trainingProgramId, int pilotId)
        {
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId)
            {
                var allFlownExercises = db.AppliedExercises.Where(x => x.Grading != null).ToList();
                var allTrainingFlights = GetTrainingFlightsFromIds(allFlownExercises.Select(x => x.FlightId).Distinct().ToList());

                var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
                var tp = trainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);
                var status = GetStatusForPilot(tp, p, allTrainingFlights, allFlownExercises);
                var details = new PilotDetailedStatus(tp, p, status.LessonsWithStatus);
                return View(details);
            }

            return View((PilotDetailedStatus)null);
        }

        private IReadOnlyList<Flight> GetTrainingFlightsFromIds(IReadOnlyList<Guid> ids)
        {
            return db.Flights.Where(x => x.Deleted == null && x.HasTrainingData && x.Date >= FirstRelevantDate && ids.Contains(x.FlightId)).ToList().Where(x=>x.IsCurrentClubPilots()).ToList();
        }

        private IEnumerable<TrainingProgramStatus> GetStatusForPilot(Pilot p, IEnumerable<Flight> allFlights, IEnumerable<AppliedExercise> allExercises)
        {
            var sw = Stopwatch.StartNew();
            var flightsByPilot = allFlights
                .Where(x => x.Date >= FirstRelevantDate)
                .Where(f => f.Pilot.PilotId == p.PilotId)
                .Select(x => new { x.FlightId, x.Departure, x.Landing, x.Date, IsTwoSeat = x.PilotBackseatId != null, x.LandingCount}) 
                .ToList();

            var model = new List<TrainingProgramStatus>();
            var flights = flightsByPilot
                .Select(x => new LightWeightFlight(x.FlightId, x.Departure, x.Landing, x.Date, x.IsTwoSeat, x.LandingCount ))
                .OrderByDescending(x => x.Timestamp)
                .ToList();
            developerInfo.Add($"__got {flights.Count} flights for pilot {p.Name} in {sw.Elapsed}");

            foreach (var program in trainingPrograms)
            {
                var m = GetStatusForPilot(program, p, flights, allExercises);
                if (m != null)
                    model.Add(m);
            }
            developerInfo.Add($"__got {model.Count} TP statuses for pilot {p.Name} in {sw.Elapsed}");

            return model;
        }

        private TrainingProgramStatus GetStatusForPilot(Training2Program tp, Pilot p, IEnumerable<Flight> allFlights, IEnumerable<AppliedExercise> allExercises)
        {
            var flightsByPilot = allFlights
                .Where(x => x.Date >= FirstRelevantDate)
                .Where(f => f.Pilot.PilotId == p.PilotId)
                .Select(x=>new {x.FlightId, x.Departure, x.Landing, x.Date, IsTwoSeat = x.PilotBackseatId != null, x.LandingCount })
                .ToList();

            var flights = flightsByPilot
                .Select(x => new LightWeightFlight(x.FlightId, x.Departure, x.Landing, x.Date, x.IsTwoSeat, x.LandingCount))
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            var status = GetStatusForPilot(tp, p, flights, allExercises);
            return status;
        }


        private TrainingProgramStatus GetStatusForPilot(Training2Program program, Pilot p, IReadOnlyList<LightWeightFlight> flightsByPilot, IEnumerable<AppliedExercise> allExercises)
        {
  				var sw = Stopwatch.StartNew();
                var flightIdsByThisPilot = flightsByPilot.Select(x => x.FlightId).ToList();
                var flowExercisesInThisProgramByThisPilot = allExercises
                    .Where(x=>flightIdsByThisPilot.Contains(x.FlightId) 
                           && x.Program.Training2ProgramId == program.Training2ProgramId)
                    .Select(ae => new LightWeightFlownExercise(ae.FlightId, ae.Lesson.Training2LessonId, ae.Exercise.Training2ExerciseId, ae.Lesson.Name, ae.Exercise.Name, ae.Grading))
                    //.DistinctBy(x=>x.FlightId)
                    .ToList();

			    developerInfo.Add($"____got {flowExercisesInThisProgramByThisPilot.Count} training flights in {program.ShortName} for {p.Name} in {sw.Elapsed}");
                if (!flowExercisesInThisProgramByThisPilot.Any())
                    return null;

                var lessonStatus = new List<LessonWithStatus>();

                foreach (var lesson in program.Lessons)
                {
                    var flownExercisesForThisLesson = flowExercisesInThisProgramByThisPilot
                        .Where(x => x.Training2LessonId == lesson.Training2LessonId)
                        .ToList();

                    var statusForExercises = new List<ExerciseWithStatus>();
                    foreach (var e in lesson.Exercises)
                    {
                        var regression = false;
                        var statusForThisExercise = TrainingStatus.NotStarted;
                        if (flownExercisesForThisLesson.Any())
                        {
                            var flownExercisesForThisExercise = flownExercisesForThisLesson
                                .Where(y => y.Training2ExerciseId == e.Training2ExerciseId)
                                .ToList();
                            if (flownExercisesForThisExercise.Any(y => y.Grading?.IsOk ?? false))
                            {
                                // at some point, got an Ok
                                statusForThisExercise = TrainingStatus.Completed;
                                // if latest flight with this exercise it not Completed, then regression is present
                                var flightIdsForThisExercise = flownExercisesForThisExercise
                                    .Select(x => x.FlightId)
                                    .Distinct()
                                    .ToList();
                                var idOfLatestFlightWithThisExercise = flightsByPilot
                                    .Where(f => flightIdsForThisExercise.Contains(f.FlightId))
                                    .OrderBy(x => x.Timestamp)
                                    .Select(x => x.FlightId)
                                    .LastOrDefault();
                                if (idOfLatestFlightWithThisExercise != Guid.Empty)
                                {
                                    var ex = flownExercisesForThisExercise
                                        .FirstOrDefault(x => x.FlightId == idOfLatestFlightWithThisExercise); //Note: Should be SingleOrDefault, but duplicates have been observed in the database (same exercise and grading twice in same flight). No harm done to just pick one.
                                    if (ex?.Grading != null && !ex.Grading.IsOk)
                                    {
                                        regression = true;
                                    }
                                }
                            }
                            else if (flownExercisesForThisExercise.Any(y => y.Grading != null))
                                statusForThisExercise = TrainingStatus.InProgress;
                        }

                        statusForExercises.Add(new ExerciseWithStatus(e, statusForThisExercise, regression));
                    }

                    lessonStatus.Add(new LessonWithStatus(lesson, statusForExercises));
                    developerInfo.Add($"______got {statusForExercises.Count} partex statuses for {lesson.Name} in {sw.Elapsed}");
                }

                if (lessonStatus.Any(x=>x.Status != TrainingStatus.NotStarted))
                {
                    var trainingFlightIdsInThisProgramByThisPilot =
                        flowExercisesInThisProgramByThisPilot.Select(x => x.FlightId).Distinct();
                    var firstDate = DateTime.Now - TimeSpan.FromDays(60);
                    var dualFlights = flightsByPilot
                        .Where(x => x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .ToList();
                    var soloFlights = flightsByPilot
                        .Where(x => !x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .ToList();
                    var recentFlights = flightsByPilot
                        .Where(x => x.Timestamp > firstDate && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .ToList();
                    var dualTime = dualFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var soloTime = soloFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var recentTime = recentFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var programStatus = new TrainingProgramStatus(
                        p,
                        program,
                        flightsByPilot,
                        flowExercisesInThisProgramByThisPilot,
                        lessonStatus.OrderBy(x=>x.DisplayOrder),
                        TimeSpan.FromHours(recentTime),
                        recentFlights.Count(),
                        TimeSpan.FromHours(dualTime),
                        dualFlights.Sum(x=>x.LandingCount),
                        TimeSpan.FromHours(soloTime),
                        soloFlights.Sum(x=>x.LandingCount)
                        );
                    developerInfo.Add($"____got data for {program.ShortName} for {p.Name} in {sw.Elapsed}");
                    return programStatus;
                }

                developerInfo.Add($"____got data for {program.ShortName} for {p.Name} in {sw.Elapsed}");
                return null;
        }

        public class LightWeightFlight
        {
            public Guid FlightId { get; }
            public DateTime Timestamp { get; }
            public TimeSpan Duration { get; }
            public bool IsTwoSeat { get; }
            public int LandingCount { get; }
            public LightWeightFlight(Guid id, DateTime? departure, DateTime? landing, DateTime date, bool isTwoSeat, int landingCount)
            {
                FlightId = id;
                Timestamp = landing ?? date;
                Duration = departure.HasValue && landing.HasValue ? (landing.Value - departure.Value) : TimeSpan.Zero;
                IsTwoSeat = isTwoSeat;
                LandingCount = landingCount;
            }
        }

        public class LightWeightFlownExercise
        {
            public LightWeightFlownExercise(Guid flightId, int training2LessonId, int training2ExerciseId, string lessonName, string exerciseName, Grading grading)
            {
                FlightId = flightId;
                this.Training2LessonId = training2LessonId;
                this.Training2ExerciseId = training2ExerciseId;
                LessonName = lessonName;
                ExerciseName = exerciseName;
                Grading = grading;
            }

            public Guid FlightId { get; }
            public int Training2LessonId { get; }
            public int Training2ExerciseId { get; }
            public string LessonName { get; }
            public string ExerciseName { get; }
            public Grading Grading { get; }
        }
    }

    public class TrainingProgramStatus
    {
        public int PilotId { get; }
        public string  PilotName { get; }
        public int ProgramId { get; }
        public string  ProgramName { get; }
        public string LastFlight { get; }
        public string HoursInLast60Days { get; }
        public int FlightsInLast60Days { get; }
        public string DualTime { get; }
        public int DualFlights { get; }
        public string SoloTime { get; }
        public int SoloFlights { get; }
        public List<LessonWithStatus> LessonsWithStatus { get; }

        public TrainingTimelineViewModel TrainingTimelineViewModel { get; set; }
        public TrainingProgramStatus(Pilot pilot, Training2Program program,
            IReadOnlyList<TrainingStatusController.LightWeightFlight> flightsInThisProgramByThisPilot,
            IEnumerable<TrainingStatusController.LightWeightFlownExercise> trainingFlightsInThisProgramByThisPilot,
            IEnumerable<LessonWithStatus> status, 
            TimeSpan flightTimeInLast60days, int flightsInLast60Days, TimeSpan dualTime, int dualFlights,
            TimeSpan soloTime, int soloFlights)
        {
            PilotId = pilot.PilotId;
            PilotName = pilot.Name;
            ProgramId = program.Training2ProgramId;
            ProgramName = $"{program.ShortName}";
            LessonsWithStatus = status.ToList();
            var lastFlight = flightsInThisProgramByThisPilot.FirstOrDefault()?.Timestamp;
            LastFlight = lastFlight.HasValue ? lastFlight.Value.ToShortDateString() : "";
            HoursInLast60Days = flightTimeInLast60days.ToString(@"hh\:mm");
            FlightsInLast60Days = flightsInLast60Days;
            DualTime = dualTime.ToString(@"hh\:mm");
            DualFlights = dualFlights;
            SoloTime = soloTime.ToString(@"hh\:mm"); ;
            SoloFlights = soloFlights;
            
            var mapper = new CoarseExerciseToNumberMapper(program.Lessons.SelectMany(x => x.Exercises));
            var timeSeriesOk = trainingFlightsInThisProgramByThisPilot
                .Where(x=>x.Grading is { IsOk: true })
                .Select(x=>new TimestampedValue
                    {
                        Timestamp = flightsInThisProgramByThisPilot.First(f=>f.FlightId == x.FlightId).Timestamp, 
                        Value = mapper.PartialExerciseToNumber(x.Training2ExerciseId),
                        Note = $"{x.LessonName}-{x.ExerciseName}",
                        Key = x.FlightId.ToString()
                    });
            var timeSeriesInProgress = trainingFlightsInThisProgramByThisPilot
                .Where(x=> x.Grading is { IsOk: false })
                .Select(x=>new TimestampedValue
                    {
                        Timestamp = flightsInThisProgramByThisPilot.First(f=>f.FlightId == x.FlightId).Timestamp, 
                        Value = mapper.PartialExerciseToNumber(x.Training2ExerciseId),
                        Note = $"{x.LessonName}-{x.ExerciseName}\n({x.Grading.Name})",
                        Key = x.FlightId.ToString()
                    });

            var metadata = new Dictionary<string, string>();
            metadata.Add("pilotId", PilotId.ToString());
            metadata.Add("pilotName", PilotName);
            metadata.Add("programId", ProgramId.ToString());
            metadata.Add("programName", ProgramName);
            TrainingTimelineViewModel = new TrainingTimelineViewModel{Data = new ScatterChartDataViewModel(new []
            {
                new TimestampedDataSeriesViewModel(new TimeDataSerie(timeSeriesOk, "OK", Color.Lime, Color.Lime, true, false)){PointRadius = 3, PointStyle = "rect"},
                new TimestampedDataSeriesViewModel(new TimeDataSerie(timeSeriesInProgress, "InProgress", Color.DeepSkyBlue, Color.DeepSkyBlue, true, false)){PointRadius = 3, PointStyle = "rect"}
            })
                {
                    ValueLabels = mapper.Labels, 
                    Metadata = metadata
                }
            };
        }
    }

    public class PilotDetailedStatus {
        public int PilotId { get; }
        public string PilotName { get; }
        public string ProgramName { get; }

        public IEnumerable<LessonWithStatus> Status { get; }

        public PilotDetailedStatus(Training2Program tp, Pilot p, IEnumerable<LessonWithStatus> status)
        {
            ProgramName = $"{tp.ShortName} {tp.Name}";
            PilotId = p.PilotId;
            PilotName = p.Name;
            Status = status;
        }
    }

    public class LessonWithStatus
    {
        public int LessonId { get; }
        public string LessonName { get; }
        public string LessonShortName { get; }
        public TrainingStatus Status { get; }
        public bool Regression { get; }
        public IEnumerable<ExerciseWithStatus> ExercisesWithStatus { get; }
        public int DisplayOrder { get; }

        public LessonWithStatus(Training2Lesson lesson, IEnumerable<ExerciseWithStatus> exercisesWithStatus)
        {
            ExercisesWithStatus = exercisesWithStatus.ToList();
            var intro = lesson.Purpose.FirstLine().RemoveNonAlphaNumPrefix().Trim();
            LessonName = intro.Any() ? $"{lesson.Name}-{intro}" : lesson.Name;
            LessonShortName = lesson.Name;
            LessonId = lesson.Training2LessonId;
            DisplayOrder = lesson.DisplayOrder;

            if (ExercisesWithStatus.All(x => x.Status == TrainingStatus.Completed))
                Status = TrainingStatus.Completed;
            else if (ExercisesWithStatus.All(x => x.Status == TrainingStatus.NotStarted))
                Status = TrainingStatus.NotStarted;
            else
                Status = TrainingStatus.InProgress;

            Regression = ExercisesWithStatus.Any(x=>x.Regression);
        }
    }

    public class ExerciseWithStatus
    {
        public int ExerciseId { get; }
        public string ExerciseName { get; }
        public TrainingStatus Status { get; }
        public bool Regression { get; }
        public int DisplayOrder { get; }


        public ExerciseWithStatus(Training2Exercise exercise, TrainingStatus status, bool regression)
        {
            Status = status;
            Regression = regression;
            ExerciseName = exercise.Name;
            ExerciseId = exercise.Training2ExerciseId;
            DisplayOrder = exercise.DisplayOrder;
        }
    }


}