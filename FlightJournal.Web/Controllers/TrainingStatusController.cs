using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Controllers
{
    public class TrainingStatusController : Controller
    {
        private readonly FlightContext db = new FlightContext();
        // GET: TrainingStatus
        public ActionResult Index()
        {
            var model = new List<TrainingProgramStatus>();
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                var flights = db.Flights.Where(f =>
                    ClubController.CurrentClub.ShortName == null
                    || f.StartedFromId == ClubController.CurrentClub.LocationId
                    || f.LandedOnId == ClubController.CurrentClub.LocationId
                    || (f.Pilot != null && f.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.PilotBackseat != null && f.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                    || (f.Betaler != null && f.Betaler.ClubId == ClubController.CurrentClub.ClubId));

                var flyingPilots = flights.Select(x => x.Pilot).Distinct().OrderBy(p=>p.Name);
                

                foreach ( var p in flyingPilots)
                {
                    model.AddRange(GetStatusForPilot(p));
                }

            }
            else if (Request.IsPilot())
            {
                model.AddRange(GetStatusForPilot(Request.Pilot()));
            }
            else
            {
                // no access
            }

            return View(model);
        }


        public ActionResult PilotStatusDetails(int trainingProgramId, int pilotId)
        {
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor || Request.Pilot().PilotId == pilotId)
            {
                var p = db.Pilots.SingleOrDefault(x => x.PilotId == pilotId);
                var tp = db.TrainingPrograms.SingleOrDefault(x => x.Training2ProgramId == trainingProgramId);
                var status = GetStatusForPilot(tp, p);
                var details = new PilotDetailedStatus(tp, p, status.LessonsWithStatus);
                return View(details);
            }

            return View((PilotDetailedStatus)null);
        }

        private IEnumerable<TrainingProgramStatus> GetStatusForPilot(Pilot p)
        {
            var flightsByPilot = db.Flights
                .Where(f => f.Pilot.PilotId == p.PilotId)
                .OrderByDescending(f => f.Landing ?? f.Date);

            var model = new List<TrainingProgramStatus>();
            foreach (var program in db.TrainingPrograms)
            {
                var m = GetStatusForPilot(program, p, flightsByPilot);
                if (m != null)
                    model.Add(m);
            }

            return model;
        }

        private TrainingProgramStatus GetStatusForPilot(Training2Program tp, Pilot p)
        {
            var flightsByPilot = db.Flights
                .Where(f => f.Pilot.PilotId == p.PilotId)
                .OrderByDescending(f => f.Landing ?? f.Date);
            var status = GetStatusForPilot(tp, p, flightsByPilot);
            return status;
        }

        private TrainingProgramStatus GetStatusForPilot(Training2Program program, Pilot p, IQueryable<Flight> flightsByPilot)
        {
            var flightIds = flightsByPilot.Select(x => x.FlightId).ToList();
                var exercisesInThisProgram = db.AppliedExercises.Where(x => flightIds.Contains(x.FlightId) && x.Program.Training2ProgramId == program.Training2ProgramId).Select(f=>f.FlightId).ToList();

                if (!exercisesInThisProgram.Any())
                    return null;

                var lessonStatus = new List<LessonWithStatus>();
                foreach (var lesson in program.Lessons)
                {
                    var flownExercisesForThisLesson = db.AppliedExercises.Where(x => flightIds.Contains(x.FlightId) && x.Lesson.Training2LessonId == lesson.Training2LessonId).Select(x=>new{ExId = x.Exercise.Training2ExerciseId, Grading = x.Grading, FlightId = x.FlightId}).ToList();

                    var statusForExercises = new List<ExerciseWithStatus>();
                    foreach (var e in lesson.Exercises)
                    {
                        var regression = false;
                        var statusForThisExercise = TrainingStatus.NotStarted;
                        var flownExercisesForThisExercise = flownExercisesForThisLesson.Where(y => y.ExId == e.Training2ExerciseId).ToList();
                        if (flownExercisesForThisExercise.Any(y => y.Grading?.IsOk ?? false))
                        {
                            statusForThisExercise = TrainingStatus.Completed;
                            // if latest flight with this exercise it not Completed, then regression is present
                            var flightIdsForThisExercise = flownExercisesForThisExercise.Select(x=>x.FlightId).ToList();
                            var idOfLatestFlightWithThisExercise = flightsByPilot
                                .Where(f => flightIdsForThisExercise.Contains(f.FlightId))
                                .OrderBy(x => x.Landing ?? x.Date)
                                .Select(x=>x.FlightId)
                                .ToList()
                                .LastOrDefault();
                            if (idOfLatestFlightWithThisExercise != Guid.Empty)
                            {
                                var ex = flownExercisesForThisExercise.SingleOrDefault(x => x.FlightId == idOfLatestFlightWithThisExercise);
                                if (ex?.Grading != null && !ex.Grading.IsOk)
                                {
                                    regression = true;
                                }
                            }
                        }
                        else if (flownExercisesForThisExercise.Any(y => y.Grading != null))
                            statusForThisExercise = TrainingStatus.InProgress;
                        statusForExercises.Add(new ExerciseWithStatus(e, statusForThisExercise, regression));
                    }

                    lessonStatus.Add(new LessonWithStatus(lesson, statusForExercises));
                }

                if (lessonStatus.Any(x=>x.Status != TrainingStatus.NotStarted))
                {
                    var firstDate = DateTime.Now - TimeSpan.FromDays(60);
                    var recentFlightsInThisProgram = flightsByPilot.Where(x => x.Date > firstDate && exercisesInThisProgram.Contains(x.FlightId));

                    var recentTime = recentFlightsInThisProgram.ToList().Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var programStatus = new TrainingProgramStatus(
                        p,
                        program,
                        lessonStatus.OrderBy(x=>x.DisplayOrder),
                        flightsByPilot.First().Date,
                        TimeSpan.FromHours(recentTime),
                        recentFlightsInThisProgram.Count()
                        );
                    return programStatus;
                }
            
            return null;
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
        public List<LessonWithStatus> LessonsWithStatus { get; }

        public TrainingProgramStatus(Pilot pilot, Training2Program program, IEnumerable<LessonWithStatus> status, DateTime? lastFlight, TimeSpan flightTimeInLast60days, int flightsInLast60Days)
        {
            PilotId = pilot.PilotId;
            PilotName = pilot.Name;
            ProgramId = program.Training2ProgramId;
            ProgramName = $"{program.ShortName} {program.Name}";
            LessonsWithStatus = status.ToList();
            LastFlight = lastFlight.HasValue ? lastFlight.Value.ToShortDateString() : "";
            HoursInLast60Days = flightTimeInLast60days.ToString(@"hh\:mm");
            FlightsInLast60Days = flightsInLast60Days;
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