using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Boerman.Core.Helpers;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

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
        private readonly FlightContext db;
        //private readonly List<Training2Exercise> trainingExercises;
        //private readonly List<Training2Lesson> trainingLessons;
        private readonly IReadOnlyList<Training2Program> trainingPrograms;
        private readonly IReadOnlyList<Pilot> pilots;
        private readonly Club CurrentClub;
        private readonly IReadOnlyList<LightWeightFlight> allFlights;

        private readonly DateTime FirstRelevantDate = DateTime.Now.AddYears(-3); // flights before this are hardly relevant

        private readonly IReadOnlyList<string> developerInfo = new List<string>();

        public TrainingStatusController()
        {
            var sw = Stopwatch.StartNew();
            db = new FlightContext();
            //trainingExercises = db.TrainingExercises.ToList();
            //trainingLessons =  db.TrainingLessons.ToList();
            trainingPrograms = db.TrainingPrograms.AsReadOnlyList();
            pilots = db.Pilots.AsReadOnlyList();
            CurrentClub = ClubController.CurrentClub;
            var fullFlights = db.Flights.Where(x => x.Deleted == null && x.HasTrainingData && x.Date >= FirstRelevantDate).AsReadOnlyList(); 
            allFlights = fullFlights.Select(x=>new LightWeightFlight(x.PilotId, x.FlightId, x.Departure, x.Landing, x.Date, x.PilotBackseatId != null, x.LandingCount)).AsReadOnlyList();
            fullFlights = null;

            Trace.WriteLine($"## TrainingStatusController() took {sw.Elapsed}"); 
        }
        // GET: TrainingStatus
        public ActionResult Index()
        {
            var sw = Stopwatch.StartNew();
            var model = new List<TrainingProgramStatus>();

            var allFlownExercises = db.AppliedExercises.Where(x => x.Grading != null).AsReadOnlyList(); // (1.3s)  // ToList() here VERY important for speed. However, it does NOT pay to load the full join and cache it.
            var trainingFlightIds = allFlownExercises.Select(x => x.FlightId).Distinct().AsReadOnlyList();
            var allTrainingFlights = GetFlightsFromIds(trainingFlightIds).AsReadOnlyList();
            Trace.WriteLine($"## got {allTrainingFlights.Count} flights and {allFlownExercises.Count} exercises in {sw.Elapsed}"); // 2.6s

            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                var idsOfFlyingPilots = allTrainingFlights.Select(f => f.PilotId)
                    .Where(p=>IsActive(p))
                    .OrderBy(p => PilotNameOf(p))
                    .Distinct()
                    ;
                if (CurrentClub.ShortName != null)
                {
                    idsOfFlyingPilots = idsOfFlyingPilots.Where(p => IsInCurrentClub(p));
                }

                idsOfFlyingPilots = idsOfFlyingPilots.ToList();
                Trace.WriteLine($"## Got {idsOfFlyingPilots.Count()} pilots in {sw.Elapsed}"); // 3.7s
                var tasks = new List<Task>();
                foreach ( var p in idsOfFlyingPilots)
                {
                    model.AddRange(GetStatusForPilot(p, allTrainingFlights, allFlownExercises));
                }

                Task.WaitAll(tasks.ToArray());
                Trace.WriteLine($"## Got all status for {idsOfFlyingPilots.Count()} pilots in {sw.Elapsed} ({sw.ElapsedMilliseconds / idsOfFlyingPilots.Count()} ms/pilot)"); // 6.5s (8.5s)

            }
            else if (Request.IsPilot())
            {
                model.AddRange(GetStatusForPilot(Request.Pilot().PilotId, allTrainingFlights, allFlownExercises));
            }
            else
            {
                // no access
            }

            ViewBag.DeveloperInfo = developerInfo;
            return View(model);
        }

        private bool IsActive(int pilotId)
        {
            var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
            return p is { ExitDate: null };
        }

        private string PilotNameOf(int pilotId)
        {
            var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
            return p?.Name ?? "";
        }

        private bool IsInCurrentClub(int pilotId)
        {
            var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
            return p?.ClubId == CurrentClub.ClubId;
        }

        private List<LightWeightFlight> FlightsByThisPilot(int pilotId, IReadOnlyList<Guid> flightIds)
        {
            var allTrainingFlightsOnAllPrograms = GetFlightsFromIds(flightIds);
            var trainingFlightsByThisPilotOnAllPrograms = allTrainingFlightsOnAllPrograms
                .Where(x => x.PilotId == pilotId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
            return trainingFlightsByThisPilotOnAllPrograms;
        }

        public ActionResult PilotStatusDetails(int trainingProgramId, int pilotId)
        {
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId)
            {  
                var allFlownExercisesOnAllPrograms = db.AppliedExercises.Where(x => x.Grading != null).ToList(); //TODO: try ToList() here
                var allTrainingFlightIds = allFlownExercisesOnAllPrograms.Select(x => x.FlightId).Distinct().ToList();

                var trainingFlightsByThisPilotOnAllPrograms = FlightsByThisPilot(pilotId, allTrainingFlightIds);

                var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
                var tp = trainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);

                var status = GetStatusForPilot(tp, pilotId, trainingFlightsByThisPilotOnAllPrograms, allFlownExercisesOnAllPrograms);
                var details = new PilotDetailedStatus(tp, p, status.LessonsWithStatus);
                return View(details);
            }

            return View((PilotDetailedStatus)null);
        }

        public ActionResult PilotActivityTimeline(int trainingProgramId, int pilotId)
        {
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId)
            {
                var flownExercisesOnThisProgram = db.AppliedExercises.Where(x => x.Grading != null && x.Program.Training2ProgramId == trainingProgramId); //TODO: try ToList() here
                var trainingFlightIdsOnThisProgram = flownExercisesOnThisProgram.Select(x=>x.FlightId).Distinct().AsReadOnlyList();

                var trainingFlightsByThisPilotOnThisProgram = FlightsByThisPilot(pilotId, trainingFlightIdsOnThisProgram);
                var flightIdsByThisPilotOnThisProgram = trainingFlightsByThisPilotOnThisProgram.Select(x => x.FlightId).ToHashSet();

                var flowExercisesInThisProgramByThisPilot = flownExercisesOnThisProgram
                    .Where(x => flightIdsByThisPilotOnThisProgram.Contains(x.FlightId))
                    .Select(ae => new LightWeightFlownExercise(ae.FlightId, ae.Program.Training2ProgramId, ae.Lesson.Training2LessonId, ae.Exercise.Training2ExerciseId, ae.Lesson.Name, ae.Exercise.Name, ae.Grading))
                .AsReadOnlyList();

                var mapper = new CoarseExerciseToNumberMapper(db.TrainingExercises.AsReadOnlyList());

                var model = GetTrainingTimelineForPilot(pilotId, trainingProgramId, mapper, trainingFlightsByThisPilotOnThisProgram, flowExercisesInThisProgramByThisPilot);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private IReadOnlyList<LightWeightFlight> GetFlightsFromIds(IReadOnlyList<Guid> ids)
        {
            return allFlights.Where(x => ids.Contains(x.FlightId)).AsReadOnlyList();
        }

        private IEnumerable<TrainingProgramStatus> GetStatusForPilot(int pilotId, IEnumerable<LightWeightFlight> flights, IReadOnlyList<AppliedExercise> allExercises)
        {
            var sw = Stopwatch.StartNew();

            var model = new List<TrainingProgramStatus>();
            var flightsByThisPilot = flights
                .Where(x=>x.PilotId == pilotId)
                .OrderByDescending(x => x.Timestamp)
                .AsReadOnlyList();

            foreach (var program in trainingPrograms)
            {
                var m = GetStatusForPilot(program, pilotId, flightsByThisPilot, allExercises);
                if (m != null)
                    model.Add(m);
            }
            Trace.WriteLine($"Got all status for {pilots.SingleOrDefault(x=>x.PilotId == pilotId)?.Name} in {sw.Elapsed}");
            return model;
        }

        //private TrainingProgramStatus GetStatusForPilot(Training2Program tp, int pilotId, IEnumerable<Flight> allFlights, IEnumerable<AppliedExercise> allExercises)
        //{
        //    var flightsByPilot = allFlights
        //        .Where(x => x.Date >= FirstRelevantDate)
        //        .Where(f => f.PilotId == pilotId)
        //        .Select(x=>new {x.FlightId, x.Departure, x.Landing, x.Date, IsTwoSeat = x.PilotBackseatId != null, x.LandingCount })
        //        .ToList();

        //    var flights = flightsByPilot
        //        .Select(x => new LightWeightFlight(pilotId, x.FlightId, x.Departure, x.Landing, x.Date, x.IsTwoSeat, x.LandingCount))
        //        .OrderByDescending(x => x.Timestamp)
        //        .ToList();

        //    var status = GetStatusForPilot(tp, pilotId, flights, allExercises);
        //    return status;
        //}


        private TrainingProgramStatus GetStatusForPilot(Training2Program program, int pilotId, IReadOnlyList<LightWeightFlight> trainingFlightsByPilot, IReadOnlyList<AppliedExercise> allFlownExercises)
        {
  				var sw = Stopwatch.StartNew();
                var flightIdsByThisPilot = trainingFlightsByPilot.Select(x => x.FlightId).ToHashSet();
                var flownExercisesOnThisProgram =
                    allFlownExercises
                        .Where(x => x.Program.Training2ProgramId == program.Training2ProgramId)
                        .AsReadOnlyList();
                var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);

                if (!flownExercisesOnThisProgram.Any())
                {
                    Trace.WriteLine($"GetStatusForPilot({p.Name}, {program.ShortName}) took {sw.ElapsedMilliseconds} ms (empty program)");
                    return null;
                }
                var flowExercisesInThisProgramByThisPilot = flownExercisesOnThisProgram
                    .Where(x=>flightIdsByThisPilot.Contains(x.FlightId))
                    .AsReadOnlyList();


                if (!flowExercisesInThisProgramByThisPilot.Any())
                {
                    Trace.WriteLine($"GetStatusForPilot({p.Name}, {program.ShortName}) took {sw.ElapsedMilliseconds} ms (not in program)");
                    return null;
                }

                var lessonStatus = new List<LessonWithStatus>();

                foreach (var lesson in program.Lessons) // query, cached
                {
                    var flownExercisesForThisLesson = flowExercisesInThisProgramByThisPilot
                        .Where(x => x.Lesson.Training2LessonId == lesson.Training2LessonId)
                        .AsReadOnlyList();

                    var statusForExercises = new List<ExerciseWithStatus>();
                    foreach (var e in lesson.Exercises) // query, cached
                    {
                        var regression = false;
                        var statusForThisExercise = TrainingStatus.NotStarted;
                        if (flownExercisesForThisLesson.Any())
                        {
                            var flownExercisesForThisExercise = flownExercisesForThisLesson
                                .Where(y => y.Exercise.Training2ExerciseId == e.Training2ExerciseId)
                                .AsReadOnlyList();
                            if (flownExercisesForThisExercise.Any(y => y.Grading?.IsOk ?? false)) // query, cached
                            {
                                // at some point, got an Ok
                                statusForThisExercise = TrainingStatus.Completed;
                                // if latest flight with this exercise it not Completed, then regression is present
                                var flightIdsForThisExercise = flownExercisesForThisExercise
                                    .Select(x => x.FlightId)
                                    .Distinct()
                                    .AsReadOnlyList();
                                var idOfLatestFlightWithThisExercise = trainingFlightsByPilot
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
                }


                if (p != null && lessonStatus.Any(x=>x.Status != TrainingStatus.NotStarted))
                {
                    var trainingFlightIdsInThisProgramByThisPilot =
                        flowExercisesInThisProgramByThisPilot.Select(x => x.FlightId).Distinct();
                    var firstDate = DateTime.Now - TimeSpan.FromDays(60);
                    var dualFlights = trainingFlightsByPilot
                        .Where(x => x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var soloFlights = trainingFlightsByPilot
                        .Where(x => !x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var recentFlights = trainingFlightsByPilot
                        .Where(x => x.Timestamp > firstDate && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var dualTime = dualFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var soloTime = soloFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var recentTime = recentFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var programStatus = new TrainingProgramStatus(p,
                        program,
                        trainingFlightsByPilot,
                        lessonStatus.OrderBy(x=>x.DisplayOrder),
                        TimeSpan.FromHours(recentTime),
                        recentFlights.Count(),
                        TimeSpan.FromHours(dualTime),
                        dualFlights.Sum(x=>x.LandingCount),
                        TimeSpan.FromHours(soloTime),
                        soloFlights.Sum(x=>x.LandingCount)
                        );

                    Trace.WriteLine($"GetStatusForPilot({p.Name}, {program.ShortName}) took {sw.ElapsedMilliseconds} ms");
                    return programStatus;
                }

                Trace.WriteLine($"GetStatusForPilot({p.Name}, {program.ShortName}) took {sw.ElapsedMilliseconds} ms (null)");
            return null;
        }

        private TrainingTimelineViewModel GetTrainingTimelineForPilot(
            int pilotId, 
            int programId, 
            CoarseExerciseToNumberMapper coarseExerciseToNumberMapper,
            IReadOnlyList<LightWeightFlight> flightsInThisProgramByThisPilot,
            IEnumerable<LightWeightFlownExercise> flownExercisesInThisProgramByThisPilot
            )
        {

            var timeSeriesOk = flownExercisesInThisProgramByThisPilot
                .Where(x => x.Grading is { IsOk: true })
                .Select(x => new TimestampedValue
                {
                    Timestamp = flightsInThisProgramByThisPilot.First(f => f.FlightId == x.FlightId).Timestamp,
                    Value = coarseExerciseToNumberMapper.PartialExerciseToNumber(x.Training2ExerciseId),
                    Note = $"{x.LessonName}-{x.ExerciseName}",
                    Key = x.FlightId.ToString()
                });
            var timeSeriesInProgress = flownExercisesInThisProgramByThisPilot
                .Where(x => x.Grading is { IsOk: false })
                .Select(x => new TimestampedValue
                {
                    Timestamp = flightsInThisProgramByThisPilot.First(f => f.FlightId == x.FlightId).Timestamp,
                    Value = coarseExerciseToNumberMapper.PartialExerciseToNumber(x.Training2ExerciseId),
                    Note = $"{x.LessonName}-{x.ExerciseName}\n({x.Grading.Name})",
                    Key = x.FlightId.ToString()
                });

            var metadata = new Dictionary<string, string>
            {
                { "pilotId", pilotId.ToString() },
                { "pilotName", pilots.SingleOrDefault(x => x.PilotId == pilotId)?.Name ?? "??" },
                { "programId", programId.ToString() },
                { "programName", trainingPrograms .SingleOrDefault(x => x.Training2ProgramId== programId)?.ShortName ?? "??" }
            };
            var model = new TrainingTimelineViewModel
            {
                Data = new ScatterChartDataViewModel(new[]
            {
                new TimestampedDataSeriesViewModel(new TimeDataSerie(timeSeriesOk, "OK", Color.Lime, Color.Lime, true, false)){PointRadius = 3, PointStyle = "rect"},
                new TimestampedDataSeriesViewModel(new TimeDataSerie(timeSeriesInProgress, "InProgress", Color.DeepSkyBlue, Color.DeepSkyBlue, true, false)){PointRadius = 3, PointStyle = "rect"}
            })
                {
                    ValueLabels = coarseExerciseToNumberMapper.Labels,
                    Metadata = metadata
                }
            };
            return model;
        }

        public class LightWeightFlight
        {
            public Guid FlightId { get; }
            public int PilotId { get; }
            public DateTime Timestamp { get; }
            public TimeSpan Duration { get; }
            public bool IsTwoSeat { get; }
            public int LandingCount { get; }
            public LightWeightFlight(int pilotId, Guid id, DateTime? departure, DateTime? landing, DateTime date, bool isTwoSeat, int landingCount)
            {
                PilotId = pilotId;
                FlightId = id;
                Timestamp = landing ?? date;
                Duration = departure.HasValue && landing.HasValue ? (landing.Value - departure.Value) : TimeSpan.Zero;
                IsTwoSeat = isTwoSeat;
                LandingCount = landingCount;
            }
        }

        public class LightWeightFlownExercise
        {
            public LightWeightFlownExercise(Guid flightId, int training2ProgramId, int training2LessonId, int training2ExerciseId, string lessonName, string exerciseName, Grading grading)
            {
                FlightId = flightId;
                Training2ProgramId = training2ProgramId;
                Training2LessonId = training2LessonId;
                Training2ExerciseId = training2ExerciseId;
                LessonName = lessonName;
                ExerciseName = exerciseName;
                Grading = grading;
            }

            public Guid FlightId { get; }
            public int Training2ProgramId { get; }
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

        public TrainingProgramStatus(Pilot pilot,
            Training2Program program,
            IReadOnlyList<TrainingStatusController.LightWeightFlight> flightsInThisProgramByThisPilot,
            IEnumerable<LessonWithStatus> status,
            TimeSpan flightTimeInLast60days, int flightsInLast60Days, TimeSpan dualTime, int dualFlights,
            TimeSpan soloTime, int soloFlights)
        {
            PilotId = pilot.PilotId;
            PilotName = pilot.Name;
            ProgramId = program.Training2ProgramId;
            ProgramName = $"{program.ShortName}";
            LessonsWithStatus = status.ToList();
            var lastFlight = flightsInThisProgramByThisPilot.OrderBy(x=>x.Timestamp).LastOrDefault()?.Timestamp;
            LastFlight = lastFlight.HasValue ? lastFlight.Value.ToShortDateString() : "";
            HoursInLast60Days = flightTimeInLast60days.ToString(@"hh\:mm");
            FlightsInLast60Days = flightsInLast60Days;
            DualTime = dualTime.ToString(@"hh\:mm");
            DualFlights = dualFlights;
            SoloTime = soloTime.ToString(@"hh\:mm"); ;
            SoloFlights = soloFlights;
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