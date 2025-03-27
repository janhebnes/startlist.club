using System;
using System.Collections.Generic;
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
using log4net;

namespace FlightJournal.Web.Controllers
{


    public class TrainingStatusController : Controller
    {
        private readonly FlightContext db;
        private readonly IReadOnlyList<Training2Program> trainingPrograms;
        private readonly IReadOnlyList<Pilot> pilots;
        private readonly IReadOnlyList<LightWeightFlight> allTrainingFlights;


        private readonly IReadOnlyList<string> developerInfo = new List<string>();

        public TrainingStatusController()
        {
            var sw = Stopwatch.StartNew();

            db = new FlightContext();
            trainingPrograms = db.TrainingPrograms.AsReadOnlyList();
            pilots = db.Pilots.AsReadOnlyList();
            var trainingFlightIds = DbHelper.IdsOfTrainingFlights(db);
            var fullFlights = db.Flights.Where(x => x.Deleted == null)
                .Select(x => new { 
                    x.PilotId, 
                    x.FlightId,
                    x.Departure,
                    x.Landing,
                    x.Date,
                    x.PilotBackseatId,
                    x.LandingCount
                } ).AsReadOnlyList(); 
            allTrainingFlights = fullFlights
                .Where(f=>trainingFlightIds.Contains(f.FlightId))
                .Select(x=>new LightWeightFlight(x.PilotId, x.FlightId, x.Departure, x.Landing, x.Date, x.PilotBackseatId != null, x.LandingCount))
                .AsReadOnlyList();

            Trace.WriteLine($"## TrainingStatusController() took {sw.Elapsed}"); 
        }


        // GET: TrainingStatus
        public ActionResult Index(bool onlyActive=true)
        {
            var sw = Stopwatch.StartNew();
            var model = new List<TrainingProgramStatus>();

            var allFlownExercises = db.AppliedExercises.AsReadOnlyList(); // ToList() here VERY important for speed. However, it does NOT pay to load the full join and cache it.

            if (User.IsAdministrator() || (Request.IsPilot() && Request.Pilot().IsInstructor))
            {
                var filteredIdsOfFlyingPilots = allTrainingFlights.GetRelevantPilotIdsFrom(pilots).ToHashSet(); // filters on club and active pilots
                var flightsByPilotId = filteredIdsOfFlyingPilots.ToDictionary(p => p, p => allTrainingFlights.WithPilot(p));
                var pilotsInTrainingPrograms = db.PilotsInTrainingPrograms.AsReadOnlyList();

                foreach (var program in trainingPrograms)
                {
                    var flownExercisesOnThisProgram = allFlownExercises.OnProgram(program);
                    if (flownExercisesOnThisProgram.Count == 0) continue; // unused program

                    var idsOfPilotsInThisTrainingProgram = pilotsInTrainingPrograms
                        .Where(x => filteredIdsOfFlyingPilots.Contains(x.PilotId))
                        .Where(x => x.Training2ProgramId == program.Training2ProgramId && (!onlyActive || x.EndDate == null))
                        .Select(x => x.PilotId)
                        .Distinct()
                        .ToHashSet();

                    foreach ( var pId in idsOfPilotsInThisTrainingProgram)
                    {
                        if (!flightsByPilotId.TryGetValue(pId, out var flightsByThisPilot) || flightsByThisPilot.Count==0) continue;

                        var  status = CreateTrainingProgramStatus(program, pId, flightsByThisPilot, flownExercisesOnThisProgram);
                        if(status!=null)
                            model.Add(status);
                    }
                }
                model = model.OrderBy(x => x.PilotName).ThenBy(x => x.ProgramName).AsList();

                Trace.WriteLine($"## Got all status for {filteredIdsOfFlyingPilots.Count} pilots in {sw.Elapsed} ({sw.ElapsedMilliseconds / filteredIdsOfFlyingPilots.Count} ms/pilot)"); // 6.5s (8.5s)

            }
            else if (Request.IsPilot())
            {
                var pilot = Request.Pilot();
                var flightsByThisPilot = allTrainingFlights.WithPilot(pilot.PilotId);
                if (flightsByThisPilot.Count > 0)
                {
                    var pilotsInTrainingPrograms = db.PilotsInTrainingPrograms.AsReadOnlyList();
                    var relevantPrograms = trainingPrograms
                        .Where(x => pilotsInTrainingPrograms
                            .Any(p => p.PilotId == pilot.PilotId && p.Training2ProgramId == x.Training2ProgramId))
                        .AsReadOnlyList();

                    foreach (var program in relevantPrograms)
                    {
                        var status = CreateTrainingProgramStatus(program, pilot.PilotId, flightsByThisPilot, allFlownExercises.OnProgram(program));
                        if (status != null)
                            model.Add(status);
                    }
                    model = model.OrderBy(x => x.ProgramName).AsList();
                }
            }
            else
            {
                // no access
            }

            ViewBag.DeveloperInfo = developerInfo;
            ViewBag.OnlyActive = onlyActive;
            return View(model);
        }


        private TrainingProgramStatus CreateTrainingProgramStatus(Training2Program program, int pilotId, IReadOnlyList<LightWeightFlight> flightsByThisPilot, IReadOnlyList<AppliedExercise> flownExercisesOnThisProgram)
        {
            if (flownExercisesOnThisProgram.Count == 0) return null; // unused program

            var flightIdsByThisPilot = flightsByThisPilot.Select(f => f.FlightId).ToHashSet();
            
            var flowExercisesInThisProgramByThisPilot = flownExercisesOnThisProgram
                .Where(x => flightIdsByThisPilot.Contains(x.FlightId))
                .AsReadOnlyList();

            if (flowExercisesInThisProgramByThisPilot.Count == 0) return null; // unused program by this pilot

            var status = GetStatusForPilot(program, pilotId, flightsByThisPilot, flowExercisesInThisProgramByThisPilot);
            return status;
        }

        // TODO: PilotStatusDetails, PilotActivityTimeline could probably benefit from refactoring (dup code). See also Index()

        public ActionResult PilotStatusDetails(int trainingProgramId, int pilotId)
        {
            if (!User.IsAdministrator() && (!Request.IsPilot() || !Request.Pilot().IsInstructor) && Request.Pilot().PilotId != pilotId) 
                return View((PilotDetailedStatus)null);

            var pilot = pilots.SingleOrDefault(x => x.PilotId == pilotId);
            var program = trainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);
            if (pilot == null || program == null) return View((PilotDetailedStatus)null);

            var trainingFlightIdsOnThisProgram = DbHelper.IdsOfTrainingFlightsWithProgramId(db, trainingProgramId);
            var allTrainingFlightsOnThisProgram = GetFlightsFromIds(trainingFlightIdsOnThisProgram).AsReadOnlyList();
            var flightsByThisPilotOnThisProgram = allTrainingFlightsOnThisProgram.WithPilot(pilotId);
            if(flightsByThisPilotOnThisProgram.Count == 0) return View((PilotDetailedStatus)null);

            var allFlownExercises = db.AppliedExercises.AsReadOnlyList(); // ToList() here VERY important for speed. However, it does NOT pay to load the full join and cache it.
            var flightIdsByThisPilotOnThisProgram = flightsByThisPilotOnThisProgram.Select(f => f.FlightId).ToHashSet();
            var flownExercisesOnThisProgram = allFlownExercises.OnProgram(trainingProgramId).AsReadOnlyList();
            var flowExercisesInThisProgramByThisPilot = flownExercisesOnThisProgram
                .Where(x => flightIdsByThisPilotOnThisProgram.Contains(x.FlightId))
                .AsReadOnlyList();
            if(flowExercisesInThisProgramByThisPilot.Count == 0) return View((PilotDetailedStatus)null);


            var status = GetStatusForPilot(program, pilotId, flightsByThisPilotOnThisProgram, flowExercisesInThisProgramByThisPilot);

            var details = new PilotDetailedStatus(program, pilot, status.LessonsWithStatus);
            return View(details);

        }

        public ActionResult PilotActivityTimeline(int trainingProgramId, int pilotId)
        {
            if (!User.IsAdministrator() && (!Request.IsPilot() || !Request.Pilot().IsInstructor) && Request.Pilot().PilotId != pilotId)
                return PartialView("_PartialTrainingTimeline", new ScatterChartDataViewModel(Enumerable.Empty<TimestampedDataSeriesViewModel>()));

            var program = trainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);
            var pilot = pilots.SingleOrDefault(x => x.PilotId == pilotId);
            if (program == null || pilot == null)
                return PartialView("_PartialTrainingTimeline", new ScatterChartDataViewModel(Enumerable.Empty<TimestampedDataSeriesViewModel>()));
            
            var flownExercisesOnThisProgram = db.AppliedExercises.Where(x => x.Program.Training2ProgramId == trainingProgramId);
            var trainingFlightIdsOnThisProgram = DbHelper.IdsOfTrainingFlightsWithProgramId(db, trainingProgramId);
            var allTrainingFlightsOnThisProgram = GetFlightsFromIds(trainingFlightIdsOnThisProgram).AsReadOnlyList();
            var flightsByThisPilotOnThisProgram = allTrainingFlightsOnThisProgram.WithPilot(pilotId);
            var flightIdsByThisPilotOnThisProgram = flightsByThisPilotOnThisProgram.Select(f => f.FlightId).ToHashSet();

            var flowExercisesInThisProgramByThisPilot = flownExercisesOnThisProgram
                .Where(x => flightIdsByThisPilotOnThisProgram.Contains(x.FlightId))
                .AsReadOnlyList()
                .Select(ae => new LightWeightFlownExercise(ae.FlightId, ae.Program.Training2ProgramId,
                    ae.Lesson.Training2LessonId, ae.Exercise.Training2ExerciseId, ae.Lesson.Name,
                    ae.Exercise.Name, ae.Grading))
                .AsReadOnlyList();


            var mapper = new CoarseExerciseToNumberMapper(program.Lessons.SelectMany(x=>x.Exercises));

            var model = GetTrainingTimelineForPilot(pilotId, trainingProgramId, mapper,
                flightsByThisPilotOnThisProgram, flowExercisesInThisProgramByThisPilot);
            return PartialView("_PartialTrainingTimeline", model.Data);
            
        }

        private IReadOnlyList<LightWeightFlight> GetFlightsFromIds(HashSet<Guid> ids)
        {
            return allTrainingFlights.Where(x => ids.Contains(x.FlightId)).AsReadOnlyList();
        }


        // TODO: consider collapsing CreateTrainingProgramStatus into this
        private TrainingProgramStatus GetStatusForPilot(Training2Program program, int pilotId, IReadOnlyList<LightWeightFlight> trainingFlightsByThisPilot, IReadOnlyList<AppliedExercise> flownExercisesOnThisProgramByThisPilot)
        {
  				var sw = Stopwatch.StartNew();
                var p = pilots.SingleOrDefault(x => x.PilotId == pilotId);
                if(p == null) 
                    return null;

                if (!flownExercisesOnThisProgramByThisPilot.Any())
                {
                    Trace.WriteLine($"GetStatusForPilot({p.Name}, {program.ShortName}) took {sw.ElapsedMilliseconds} ms (not in program)");
                    return null;
                }
                var lessonStatus = new List<LessonWithStatus>();

                foreach (var lesson in program.Lessons.AsReadOnlyList()) // query, cached
                {
                    var flownExercisesForThisLesson = flownExercisesOnThisProgramByThisPilot
                        .Where(x => x.Lesson.Training2LessonId == lesson.Training2LessonId)
                        .AsReadOnlyList();

                    var statusForExercises = new List<ExerciseWithStatus>();
                    foreach (var e in lesson.Exercises.AsReadOnlyList()) // query, cached
                    {
                        var regression = false;
                        var statusForThisExercise = TrainingStatus.NotStarted;
                        if (flownExercisesForThisLesson.Any())
                        {
                            var flownExercisesForThisExercise = flownExercisesForThisLesson
                                .Where(y => y.Exercise.Training2ExerciseId == e.Training2ExerciseId)
                                .AsReadOnlyList();
                            if (flownExercisesForThisExercise.Any(y => y.Grading.IsOk)) // query, cached
                            {
                                // at some point, got an Ok
                                statusForThisExercise = TrainingStatus.Completed;
                                // if latest flight with this exercise it not Completed, then regression is present
                                var flightIdsForThisExercise = flownExercisesForThisExercise
                                    .Select(x => x.FlightId)
                                    .Distinct()
                                    .AsReadOnlyList();
                                var idOfLatestFlightWithThisExercise = trainingFlightsByThisPilot
                                    .Where(f => flightIdsForThisExercise.Contains(f.FlightId))
                                    .OrderBy(x => x.Timestamp)
                                    .Select(x => x.FlightId)
                                    .LastOrDefault();
                                if (idOfLatestFlightWithThisExercise != Guid.Empty)
                                {
                                    var ex = flownExercisesForThisExercise.FirstOrDefault(x => x.FlightId == idOfLatestFlightWithThisExercise); //Note: Should be SingleOrDefault, but duplicates have been observed in the database (same exercise and grading twice in same flight). No harm done to just pick one.
                                    if (ex != null && !ex.Grading.IsOk)
                                    {
                                        regression = true;
                                    }
                                }
                            }
                            else if (flownExercisesForThisExercise.Any())
                                statusForThisExercise = TrainingStatus.InProgress;
                        }

                        statusForExercises.Add(new ExerciseWithStatus(e, statusForThisExercise, regression));
                    }

                    lessonStatus.Add(new LessonWithStatus(lesson, statusForExercises));
                }


                if (lessonStatus.Any(x=>x.Status != TrainingStatus.NotStarted))
                {
                    var trainingFlightIdsInThisProgramByThisPilot =
                        flownExercisesOnThisProgramByThisPilot.Select(x => x.FlightId).Distinct();
                    var firstDate = DateTime.Now - TimeSpan.FromDays(60);
                    var dualFlights = trainingFlightsByThisPilot
                        .Where(x => x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var soloFlights = trainingFlightsByThisPilot
                        .Where(x => !x.IsTwoSeat && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var recentFlights = trainingFlightsByThisPilot
                        .Where(x => x.Timestamp > firstDate && trainingFlightIdsInThisProgramByThisPilot.Contains(x.FlightId))
                        .AsReadOnlyList();
                    var dualTime = dualFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var soloTime = soloFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var recentTime = recentFlights.Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var programStatus = new TrainingProgramStatus(p,
                        program,
                        trainingFlightsByThisPilot,
                        lessonStatus.OrderBy(x=>x.DisplayOrder),
                        TimeSpan.FromHours(recentTime),
                        recentFlights.Count,
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
            IReadOnlyList<LightWeightFlownExercise> flownExercisesInThisProgramByThisPilot
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
        public string ProgramName { get; }
        public string LastFlight { get; }
        public string HoursInLast60Days { get; }
        public int FlightsInLast60Days { get; }
        public int DaysSinceLastFlight { get; }
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
            DaysSinceLastFlight = lastFlight.HasValue ? (DateTime.Now - lastFlight.Value).Days : -1;
            HoursInLast60Days = flightTimeInLast60days.ToString(@"hh\:mm");
            FlightsInLast60Days = flightsInLast60Days;
            DualTime = dualTime.ToString(@"hh\:mm");
            DualFlights = dualFlights;
            SoloTime = soloTime.ToString(@"hh\:mm");
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

    public static class TrainingStatusExtensions
    {
        public static IReadOnlyList<AppliedExercise> OnProgram(this IReadOnlyList<AppliedExercise> allFlownExercises, Training2Program program) =>
            allFlownExercises.OnProgram(program.Training2ProgramId);

        public static IReadOnlyList<AppliedExercise> OnProgram(this IReadOnlyList<AppliedExercise> allFlownExercises,
            int programId) =>
            allFlownExercises
                .Where(x => x.Program.Training2ProgramId == programId)
                .AsReadOnlyList();

        public static IReadOnlyList<TrainingStatusController.LightWeightFlight> WithPilot(
            this IReadOnlyList<TrainingStatusController.LightWeightFlight> flights, int pilotId) =>
            flights
                .Where(x => x.PilotId == pilotId)
                .AsReadOnlyList();

        public static IReadOnlyList<int> GetRelevantPilotIdsFrom(
            this IReadOnlyList<TrainingStatusController.LightWeightFlight> flights, IReadOnlyList<Pilot> pilots)
        {
            var clubId = ClubController.CurrentClub.ShortName == null
                ? int.MinValue
                : ClubController.CurrentClub.ClubId;
            var ids = flights
                .Select(f => pilots.Single(x => x.PilotId == f.PilotId))
                .Where(p => p is { ExitDate: null })
                .Where(p => clubId == int.MinValue || p.ClubId == clubId)
                .OrderBy(p => p.Name)
                .Select(p => p.PilotId)
                .Distinct()
                .AsReadOnlyList();


            return ids;
        }
    }
}