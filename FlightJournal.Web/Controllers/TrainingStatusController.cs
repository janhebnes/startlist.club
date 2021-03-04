using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        // GET: TrainingStatus
        public ActionResult Index()
        {
            var model = new List<TrainingProgramStatus>();
            if (User.IsAdministrator() || Request.IsPilot() && Request.Pilot().IsInstructor)
            {
                var flyingPilots = db.Flights.Select(x => x.Pilot).Distinct().OrderBy(p=>p.Name);
                

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

        private IEnumerable<TrainingProgramStatus> GetStatusForPilot(Pilot p)
        {
            var model = new List<TrainingProgramStatus>();
            var flightsByPilot = db.Flights
                .Where(f=>f.Pilot.PilotId == p.PilotId)
                .OrderByDescending(f => f.Landing ?? f.Date);

            var flightIds = flightsByPilot.Select(x => x.FlightId).ToList();
            foreach (var program in db.TrainingPrograms)
            {
                var flightsForThisProgram = db.AppliedExercises.Where(x => flightIds.Contains(x.FlightId) && x.Program.Training2ProgramId == program.Training2ProgramId).Select(f=>f.FlightId).ToList();
                var lessonStatus = new List<LessonWithStatus>();
                foreach (var lesson in program.Lessons)
                {
                    var flownExercisesForThisLesson = db.AppliedExercises.Where(x => flightIds.Contains(x.FlightId) && x.Lesson.Training2LessonId == lesson.Training2LessonId);
                    TrainingStatus status = TrainingStatus.NotStarted;
                    foreach (var e in lesson.Exercises)
                    {
                        var flownExercisesForThisExercise = flownExercisesForThisLesson.Where(y => y.Exercise.Training2ExerciseId == e.Training2ExerciseId);
                        if (flownExercisesForThisExercise.Any(y=> y.Action == ExerciseAction.Completed))
                            status = TrainingStatus.Completed;
                        else if (flownExercisesForThisExercise.Any(y => y.Action == ExerciseAction.Trained))
                            status = TrainingStatus.Trained;
                        else if (flownExercisesForThisExercise.Any(y => y.Action == ExerciseAction.Briefed))
                            status = e.IsBriefingOnly ? TrainingStatus.Completed : TrainingStatus.Briefed;
                    }

                    lessonStatus.Add(new LessonWithStatus(lesson, status ));
                }

                if (lessonStatus.Any(x=>x.Status != TrainingStatus.NotStarted))
                {
                    var firstDate = DateTime.Now - TimeSpan.FromDays(60);
                    var recentFlightsInThisProgram = flightsByPilot.Where(x => x.Date > firstDate && flightsForThisProgram.Contains(x.FlightId));

                    var recentTime = recentFlightsInThisProgram.ToList().Select(y => y.Duration).Select(x => x.TotalHours).Sum();
                    var programStatus = new TrainingProgramStatus(
                        p,
                        program,
                        lessonStatus,
                        flightsByPilot.First().Date,
                        TimeSpan.FromHours(recentTime),
                        recentFlightsInThisProgram.Count()
                        );
                    model.Add(programStatus);
                }
            }

            return model;
        }

    }

    public class TrainingProgramStatus
    {
        public string  PilotName { get; }
        public string  ProgramName { get; }
        public string LastFlight { get; }
        public string HoursInLast60Days { get; }
        public int FlightsInLast60Days { get; }
        public List<LessonWithStatus> LessonsWithStatus { get; }

        public TrainingProgramStatus(Pilot pilot, Training2Program program, IEnumerable<LessonWithStatus> status, DateTime? lastFlight, TimeSpan flightTimeInLast60days, int flightsInLast60Days)
        {
            PilotName = pilot.Name;
            ProgramName = program.Name;
            LessonsWithStatus = status.ToList();
            LastFlight = lastFlight.HasValue ? lastFlight.Value.ToShortDateString() : "";
            HoursInLast60Days = flightTimeInLast60days.ToString(@"hh\:mm");
            FlightsInLast60Days = flightsInLast60Days;
        }
    }

    public class LessonWithStatus
    {
        public string LessonName { get;  }
        public TrainingStatus Status { get; }
        public LessonWithStatus(Training2Lesson lesson, TrainingStatus status)
        {
            Status = status;
            LessonName = lesson.Name;
        }
    }
}