using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;

namespace FlightJournal.Web.Controllers
{
    public class TrainingStatusController : Controller
    {
        private readonly FlightContext db = new FlightContext();

        private readonly DateTime FirstRelevantDate = DateTime.Now.AddYears(-3); // flights before this are hardly relevant

        private List<string> developerInfo = new List<string>();

        // GET: TrainingStatus
        public ActionResult Index()
        {
            var sw = Stopwatch.StartNew();

            var model = new List<TrainingProgramStatus>();
            // pay up front
            var allFlownExercises = db.AppliedExercises.Where(x => x.Grading != null).ToList();
            var allTrainingFlights = GetTrainingFlightsFromIds(allFlownExercises.Select(x => x.FlightId).Distinct().ToList());

            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                List<Pilot> flyingPilots;
                if (ClubController.CurrentClub.ShortName == null)
                {
                    flyingPilots = allTrainingFlights.Select(f => f.Pilot)
                        .OrderBy(p => p.Name)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    flyingPilots = allTrainingFlights.Select(f => f.Pilot)
                        .Where(p => p.ClubId == ClubController.CurrentClub.ClubId)
                        .OrderBy(p => p.Name)
                        .Distinct()
                        .ToList();
                }
                developerInfo.Add($"Got {flyingPilots.Count()} pilots in {sw.Elapsed}");
                foreach ( var p in flyingPilots)
                {
                    model.AddRange(GetStatusForPilot(p, allTrainingFlights, allFlownExercises));
                }
                developerInfo.Add($"Got all status in {sw.Elapsed}");

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

                var p = db.Pilots.SingleOrDefault(x => x.PilotId == pilotId);
                var tp = db.TrainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);
                var status = GetStatusForPilot(tp, p, allTrainingFlights, allFlownExercises);
                var details = new PilotDetailedStatus(tp, p, status.LessonsWithStatus);
                return View(details);
            }

            return View((PilotDetailedStatus)null);
        }

        private IEnumerable<Flight> GetTrainingFlightsFromIds(IReadOnlyList<Guid> ids)
        {
            return db.Flights.Where(x => x.Deleted == null && ids.Contains(x.FlightId)).ToList();
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

            foreach (var program in db.TrainingPrograms)
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
                var trainingFlightsInThisProgramByThisPilot = allExercises
                    .Where(x=>flightIdsByThisPilot.Contains(x.FlightId) 
                           && x.Program.Training2ProgramId == program.Training2ProgramId)
                    .Select(ae=>new {ae.FlightId, ae.Lesson.Training2LessonId, ae.Exercise.Training2ExerciseId, ae.Grading})
                    .Distinct()
                    .ToList();

			    developerInfo.Add($"____got {trainingFlightsInThisProgramByThisPilot.Count} training flights in {program.ShortName} for {p.Name} in {sw.Elapsed}");
                if (!trainingFlightsInThisProgramByThisPilot.Any())
                    return null;

                var lessonStatus = new List<LessonWithStatus>();

                foreach (var lesson in program.Lessons)
                {
                    var flownExercisesForThisLesson = trainingFlightsInThisProgramByThisPilot
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
                                        .SingleOrDefault(x => x.FlightId == idOfLatestFlightWithThisExercise);
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
                        trainingFlightsInThisProgramByThisPilot.Select(x => x.FlightId);
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
                        lessonStatus.OrderBy(x=>x.DisplayOrder),
                        flightsByPilot.First().Timestamp,
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

        private class LightWeightFlight
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

        public TrainingProgramStatus(Pilot pilot, Training2Program program, IEnumerable<LessonWithStatus> status, DateTime? lastFlight, TimeSpan flightTimeInLast60days, int flightsInLast60Days, TimeSpan dualTime, int dualFlights, TimeSpan soloTime, int soloFlights)
        {
            PilotId = pilot.PilotId;
            PilotName = pilot.Name;
            ProgramId = program.Training2ProgramId;
            ProgramName = $"{program.ShortName} {program.Name}";
            LessonsWithStatus = status.ToList();
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