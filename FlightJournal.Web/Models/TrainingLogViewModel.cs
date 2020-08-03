using System;
using System.Collections.Generic;
using System.Linq;
using FlightJournal.Web.Models.Training;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Apparently, we cannot carry the FlightContext around in EF, hence this Entity wrapper
    /// </summary>
    public class TrainingDataWrapper
    {
        //TODO: split this in a catalogue wrapper and a pilot/flight specific wrapper
        internal TrainingDataWrapper(FlightContext db, int pilotId, Guid flightId)
        {
            FlightId = flightId;
            TrainingPrograms = db.TrainingPrograms;
            TrainingLessons = db.TrainingLessons;
            TrainingExercises = db.TrainingExercises;
            TrainingProgramLessonRelations = db.TrainingProgramLessonRelations;
            TrainingLessonExerciseRelations = db.TrainingLessonExerciseRelations;

            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId).OrderBy(x=>x.Date);
            FlightAnnotations = PilotFlights.SelectMany(x => db.TrainingFlightAnnotations.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y=>x.Date));
        }

        // catalogue stuff
        public IEnumerable<Training.Training2ProgramLessonRelation> TrainingProgramLessonRelations { get; }
        public IEnumerable<Training.Training2LessonExerciseRelation> TrainingLessonExerciseRelations { get; }
        public IEnumerable<Training.Training2Program> TrainingPrograms { get; }
        public IEnumerable<Training.Training2Lesson> TrainingLessons { get; }
        public IEnumerable<Training.Training2Exercise> TrainingExercises { get; }

        // flight specific data
        public Guid FlightId { get; }
        public IEnumerable<Flight> PilotFlights{ get; }
        public IEnumerable<Training.AppliedExercise> AppliedExercises { get; } // across all PilotFlights
        public IEnumerable<Training.TrainingFlightAnnotation> FlightAnnotations{ get; } // across all PilotFlights
    }


    /// <summary>
    /// Viewmodel for an actual training flight
    /// </summary>
    public class FlightLogEntryViewModel
    {
        public DateTime Date { get; }
        public string Notes { get; }
        public string Maneuvers{ get; }
        public string StartAnnotations{ get; }
        public string FlightAnnotations{ get; }
        public string ApproachAnnotations{ get; }
        public string LandingAnnotations{ get; }
        public IEnumerable<AppliedExerciseViewModel> ExercisesWithStatus { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="date"></param>
        /// <param name="exercises">Exercises applied in this flight</param> 
        /// <param name="annotations">Annotations for this flight</param>
        public FlightLogEntryViewModel(Guid flightId, TrainingDataWrapper db, DateTime date)
        {
            Date = date;
            var annotationsForThisFlight = db.FlightAnnotations.Where(x => x.FlightId == flightId).ToList();
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flightId);
            Notes = string.Join("; ", annotationsForThisFlight.Select(x => x.Note));
            //TODO: localize / map to symbols
            Maneuvers = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.Maneuvers)));
            StartAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.StartAnnotation)));
            FlightAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.FlightAnnotation)));
            ApproachAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.ApproachAnnotation)));
            LandingAnnotations= string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.LandingAnnotation)));
            ExercisesWithStatus = exercisesForThisFlight.Select(x => new AppliedExerciseViewModel(db,x));
        }

    }

    public class FlightPhaseAnnotationViewModel
    {
        public FlightPhaseAnnotation Id { get; }
        public string Name { get; }
        public string Icon { get; }

        public FlightPhaseAnnotationViewModel(FlightPhaseAnnotation id)
        {
            Id = id;
            switch (Id)
            {
                case FlightPhaseAnnotation.Ok:
                    Name = "&#x2713";
                    break;
                case FlightPhaseAnnotation.AlmostOk:
                    Name = "(&#x2713)";
                    break;
                case FlightPhaseAnnotation.Skull:
                    Name = "&#x2620";
                    break;
                default:
                    Name = Id.ToString();
                    break;
            }
        }
    }
    public class FlightManeuverViewModel
    {
        public FlightManeuver Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public FlightManeuverViewModel(FlightManeuver id)
        {
            Id = id;
            switch (id)
            {
                case FlightManeuver.Left90:
                case FlightManeuver.Right90:
                    Name = "90";
                    break;
                case FlightManeuver.Left180:
                case FlightManeuver.Right180:
                    Name = "180";
                    break;
                case FlightManeuver.Left360:
                case FlightManeuver.Right360:
                    Name = "360";
                    break;
                case FlightManeuver.FigureEight:
                    Name = "&infin;";
                    break;
                case FlightManeuver.Bank30:
                    Name = "&ang;30&deg;";
                    break;
                case FlightManeuver.Bank45:
                    Name = "&ang;45&deg;";
                    break;
                case FlightManeuver.Bank60:
                    Name = "&ang;60&deg;";
                    break;
                case FlightManeuver.AbortedStartLowAltitude:
                    Name = "&#x21B7 Afb start lavt";
                    break;
                case FlightManeuver.AbortedStartMediumAltitude:
                    Name = "&#x21B7 Afb start mellem";
                    break;
                case FlightManeuver.AbortedStartHighAltitude:
                    Name = "&#x21B7 Afb start højt";
                    break;
                case FlightManeuver.STurn:
                    Name = "&#x219D S-drej";
                    break;
                case FlightManeuver.LeftCircuit:
                    Name = "&#x21B0 Landingsrunde";
                    break;
                case FlightManeuver.RightCircuit:
                    Name = "&#x21B1 Landingsrunde";
                    break;
                default:
                    Name = id.ToString(); // TODO: localize
                    break;
            }
            switch (id)
            {
                case FlightManeuver.Left90:
                case FlightManeuver.Left180:
                case FlightManeuver.Left360:
                    Icon = "fa fa-undo";
                    break;
                case FlightManeuver.Right90:
                case FlightManeuver.Right180:
                case FlightManeuver.Right360:
                    Icon = " fa fa-repeat";
                    break;

            }
        }
    }



    public class WindSpeedViewModel
    {
        public int Value { get; }
        public string Text { get; }

        public WindSpeedViewModel(int speed)
        {
            Value = speed;
            Text = $"{speed}kn";
        }
    }

    public class WindDirectionViewModel
    {
        public int Value { get; }
        public string Text { get; }

        public WindDirectionViewModel(int direction)
        {
            Value = direction;
            Text = $"{direction}°";
        }
    }

    public class TrainingLogViewModel
    {
        public TrainingLogViewModel(DateTime date, string pilot, string backseatPilot, TrainingDataWrapper dbmodel)
        {
            Date = date;
            Pilot = pilot;
            BackseatPilot = backseatPilot;

            FlightLog = dbmodel.PilotFlights.Select(x=>new FlightLogEntryViewModel(x.FlightId, dbmodel, date));

            // catalogue stuff
            TrainingPrograms = dbmodel.TrainingPrograms.Select(x => new TrainingProgramViewModel(x, dbmodel)); // also includes some status - should be separated!
            TrainingLessons = dbmodel.TrainingLessons.Select(x => new TrainingLessonViewModel(x));
            TrainingExercises = dbmodel.TrainingExercises.Select(x => new TrainingExerciseViewModel(x));

            Maneuvers = ((FlightManeuver[])Enum.GetValues(typeof(FlightManeuver))).Select(x=>new FlightManeuverViewModel(x));
            Annotations  = ((FlightPhaseAnnotation[])Enum.GetValues(typeof(FlightPhaseAnnotation))).Select(x=>new FlightPhaseAnnotationViewModel(x));

            var wd = new List<WindDirectionViewModel>();
            for (int v = 0; v < 360; v += 45)
                wd.Add(new WindDirectionViewModel(v ));
            WindDirections = wd;

            var ws = new List<WindSpeedViewModel>();
            for (int v = 0; v < 30; v += 5)
                ws.Add(new WindSpeedViewModel(v));
            WindSpeeds = ws;

        }
        public DateTime Date { get; }
        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<FlightLogEntryViewModel> FlightLog { get; }
        public IEnumerable<TrainingProgramViewModel> TrainingPrograms { get; }
        public IEnumerable<TrainingLessonViewModel> TrainingLessons { get; }
        public IEnumerable<TrainingExerciseViewModel> TrainingExercises { get; }

        public IEnumerable<FlightManeuverViewModel> Maneuvers { get; }
        public IEnumerable<WindDirectionViewModel> WindDirections { get; }
        public IEnumerable<WindSpeedViewModel> WindSpeeds { get; }
        public IEnumerable<FlightPhaseAnnotationViewModel> Annotations{ get; }
    }

    public class TrainingProgramViewModel
    {
        public string Id => _program.Id.ToString();
        public string Name => _program.Name;

        private IEnumerable<TrainingLessonOverallStatusViewModel> _lessons;

        public IEnumerable<TrainingLessonOverallStatusViewModel> Lessons
        {
            get
            {
                if (_lessons == null)
                {
                    var rels = _db.TrainingProgramLessonRelations.Where(x => x.ProgramId == _program.Id).OrderBy(x=>x.RelativeOrder);
                    var lessons = rels.Select(x => _db.TrainingLessons.SingleOrDefault(y => y.Id == x.LessonId)).Where(z => z != null);
                    _lessons = lessons.Select(less => new TrainingLessonOverallStatusViewModel(less, _db));
                }

                return _lessons;
            }
        }


        private readonly TrainingDataWrapper _db;
        private readonly Training.Training2Program _program;

        public TrainingProgramViewModel(Training.Training2Program program, TrainingDataWrapper db)
        {
            _db = db;
            _program = program;
        }

    }

    public class TrainingLessonViewModel
    {
        public string Id => _lesson.Id.ToString();
        public string Name => _lesson.Name;
        public string Description => _lesson.Purpose;
        public string Precondition => _lesson.Precondition;

        private readonly Training.Training2Lesson _lesson;

        public TrainingLessonViewModel(Training.Training2Lesson lesson)
        {
            _lesson = lesson;
        }
    }


    public class TrainingExerciseViewModel
    {
        public string Id => _exercise.Id.ToString();
        public string Description => _exercise.Name;
        public string Note => _exercise.Note;

        private readonly Training.Training2Exercise _exercise;

        public TrainingExerciseViewModel(Training.Training2Exercise  exercise)
        {
            _exercise = exercise;
        }
    }



    public class TrainingLessonOverallStatusViewModel
    {
        public string Id => _lesson.Id.ToString();
        public string Name => _lesson.Name;

        public string Description => _lesson.Purpose;
        private IEnumerable<TrainingExerciseOverallStatusViewModel> _exercises;

        public IEnumerable<TrainingExerciseOverallStatusViewModel> Exercises
        {
            get
            {
                if (_exercises == null)
                {
                    var rels = _db.TrainingLessonExerciseRelations.Where(x => x.LessonId == _lesson.Id).OrderBy(x=>x.RelativeOrder);
                    var exercises = rels.Select(x => _db.TrainingExercises.SingleOrDefault(y => y.Id == x.ExerciseId)).Where(z => z != null);
                    _exercises = exercises.Select(ex => new TrainingExerciseOverallStatusViewModel( ex, _db));
                }
                return _exercises;
            }
        }

        public int ExercisesTotal => Exercises.Count();
        public int ExercisesCompleted => Exercises.Count(x=>x.Status == TrainingStatus.Completed);
        public int ExercisesInProgress => ExercisesTotal - ExercisesCompleted - ExercisesNotStarted;
        public int ExercisesNotStarted => Exercises.Count(x => x.Status == TrainingStatus.NotStarted);

        public string StatusSummary => $"{ExercisesNotStarted}/{ExercisesInProgress}/{ExercisesCompleted} ({ExercisesTotal})";
        public TrainingStatus Status => 
        Exercises.All(x => x.Status == TrainingStatus.NotStarted)
            ? TrainingStatus.NotStarted
            : Exercises.All(x => x.Status == TrainingStatus.Completed)
                ? TrainingStatus.Completed
                : TrainingStatus.Trained;

        private readonly TrainingDataWrapper _db;
        private readonly Training.Training2Lesson _lesson;
        public TrainingLessonOverallStatusViewModel(Training.Training2Lesson lesson, TrainingDataWrapper db)
        {
            _db = db;
            _lesson = lesson;
        }
    }



    //TODO this needs separation into
    // a 'overall status'  model (status to this day),
    // a 'flight status' model with the AppliedExercise relevant for a specific flight
    // a 'catalogue' model (no status, just the descriptions) (?)

    public class TrainingExerciseOverallStatusViewModel
    {
        public string Id => _exercise.Id.ToString();

        public string Description => _exercise.Name;
        public string LongDescription => _exercise.Note;
        public TrainingStatus Status { get; set; }

        public bool IsBriefed => Status == TrainingStatus.Briefed 
                                 || Status == TrainingStatus.Trained 
                                 || Status == TrainingStatus.Completed;
        public bool IsTrained => Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;
        public bool IsCompleted => Status == TrainingStatus.Completed;
        public bool BriefingOnlyRequired => _exercise.IsBriefingOnly;

        private readonly TrainingDataWrapper _db;
        private readonly Training.Training2Exercise _exercise;
        public TrainingExerciseOverallStatusViewModel(Training.Training2Exercise exercise, TrainingDataWrapper db)
        {
            _db = db;
            _exercise = exercise;
            var totalApplied = _db.AppliedExercises.Where(x => x.ExerciseId == _exercise.Id).ToList();
            var isBriefed = totalApplied.Any(y => y.Action == ExerciseAction.Briefed);
            var isTrained = totalApplied.Any(y => y.Action == ExerciseAction.Trained);
            var isCompleted = totalApplied.Any(y => y.Action == ExerciseAction.Completed);

            if (true) // fake it for UI demo purposes
            {
                var toss = new Random().NextDouble();
                if (toss > 0.8)
                    Status = _exercise.IsBriefingOnly ? TrainingStatus.Briefed : TrainingStatus.Completed;
                else if (toss > 0.6)
                    Status = _exercise.IsBriefingOnly ? TrainingStatus.Briefed : TrainingStatus.Trained;
                else if (toss > 0.4)
                    Status = TrainingStatus.Briefed;
                else
                    Status = TrainingStatus.NotStarted;
            }
            else
            {
                Status =
                    isCompleted ? TrainingStatus.Completed :
                    isTrained ? TrainingStatus.Trained :
                    isBriefed ? TrainingStatus.Briefed :
                    TrainingStatus.NotStarted;
            }
        }
    }


    public class AppliedExerciseViewModel
    {
        public string Description { get; }

        public ExerciseAction Action { get; set; }

        public AppliedExerciseViewModel(TrainingDataWrapper db, AppliedExercise appliedExercise)
        {
            Action = appliedExercise.Action;
            var program = db.TrainingPrograms.SingleOrDefault(x=>x.Id == appliedExercise.ProgramId);
            var lesson = db.TrainingLessons.SingleOrDefault(x=>x.Id == appliedExercise.LessonId);
            var exercise = db.TrainingExercises.SingleOrDefault(x => x.Id == appliedExercise.ExerciseId);
            Description = $"{program?.Name} {lesson?.Name} {exercise?.Name}";
        }
    }


    //TODO: add viewmodel for last 15 flights (
    //TODO: send back data to db (post): update db.AppliedExercises and db.TrainingFlightAnnotation (not created yet)
    //TODO: UI and models for SFIL quickselect

    public enum TrainingStatus
    {
        NotStarted,
        Briefed,
        Trained,
        Completed
    }
}