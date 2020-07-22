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
        internal TrainingDataWrapper(FlightContext db, int pilotId)
        {
            TrainingPrograms = db.TrainingPrograms;
            TrainingLessons = db.TrainingLessons;
            TrainingExercises = db.TrainingExercises;
            TrainingProgramLessonRelations = db.TrainingProgramLessonRelations;
            TrainingLessonExerciseRelations = db.TrainingLessonExerciseRelations;

            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId);
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y=>x.Date));
        }

        public IEnumerable<Training.Training2ProgramLessonRelation> TrainingProgramLessonRelations { get; }
        public IEnumerable<Training.Training2LessonExerciseRelation> TrainingLessonExerciseRelations { get; }
        public IEnumerable<Training.Training2Program> TrainingPrograms { get; }
        public IEnumerable<Training.Training2Lesson> TrainingLessons { get; }
        public IEnumerable<Training.Training2Exercise> TrainingExercises { get; }

        public IEnumerable<Flight> PilotFlights{ get; }
        public IEnumerable<Training.AppliedExercise> AppliedExercises { get; }
    }




    public class TrainingLogViewModel
    {
        public TrainingLogViewModel(string pilot, string backseatPilot, TrainingDataWrapper dbmodel)
        {
            Pilot = pilot;
            BackseatPilot = backseatPilot;
            TrainingPrograms = dbmodel.TrainingPrograms.Select(x => new TrainingProgramViewModel(x, dbmodel));
        }

        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<TrainingProgramViewModel> TrainingPrograms { get; set; }
    }

    public class TrainingProgramViewModel
    {
        public string Id => _program.Id.ToString();
        public string Name => _program.Name;

        private IEnumerable<TrainingLessonViewModel> _lessons;

        public IEnumerable<TrainingLessonViewModel> Lessons
        {
            get
            {
                if (_lessons == null)
                {
                    var rels = _db.TrainingProgramLessonRelations.Where(x => x.ProgramId == _program.Id).OrderBy(x=>x.RelativeOrder);
                    var lessons = rels.Select(x => _db.TrainingLessons.SingleOrDefault(y => y.Id == x.LessonId)).Where(z => z != null);
                    _lessons = lessons.Select(less => new TrainingLessonViewModel(less, _db));
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
        private IEnumerable<TrainingExerciseViewModel> _exercises;

        public IEnumerable<TrainingExerciseViewModel> Exercises
        {
            get
            {
                if (_exercises == null)
                {
                    var rels = _db.TrainingLessonExerciseRelations.Where(x => x.LessonId == _lesson.Id).OrderBy(x=>x.RelativeOrder);
                    var exercises = rels.Select(x => _db.TrainingExercises.SingleOrDefault(y => y.Id == x.ExerciseId)).Where(z => z != null);
                    _exercises = exercises.Select(ex => new TrainingExerciseViewModel( ex, _db));
                }
                return _exercises;
            }
        }

        public TrainingStatus Status => 
        Exercises.All(x => x.Status == TrainingStatus.NotStarted)
            ? TrainingStatus.NotStarted
            : Exercises.All(x => x.Status == TrainingStatus.Completed)
                ? TrainingStatus.Completed
                : TrainingStatus.Trained;

        private readonly TrainingDataWrapper _db;
        private readonly Training.Training2Lesson _lesson;
        public TrainingLessonViewModel(Training.Training2Lesson lesson, TrainingDataWrapper db)
        {
            _db = db;
            _lesson = lesson;
        }
    }

    public class TrainingExerciseViewModel
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
        public TrainingExerciseViewModel(Training.Training2Exercise exercise, TrainingDataWrapper db)
        {
            _db = db;
            _exercise = exercise;
            var thisApplied = _db.AppliedExercises.Where(x => x.ExerciseId == _exercise.Id).ToList();
            var isBriefed = thisApplied.Any(y => y.Action == ExerciseAction.Briefed);
            var isTrained = thisApplied.Any(y => y.Action == ExerciseAction.Trained);
            var isCompleted = thisApplied.Any(y => y.Action == ExerciseAction.Completed);
            Status = 
                isCompleted ? TrainingStatus.Completed :
                isTrained ? TrainingStatus.Trained :
                isBriefed ? TrainingStatus.Briefed : 
                TrainingStatus.NotStarted;
        }


        //TODO: add viewmodel for last 15 flights
        //TODO: send back data to db (post): update db.AppliedExercises and db.TrainingFLightNote (not created yet)
        //TODO: UI and models for SFIL quickselect
    }

    public enum TrainingStatus
    {
        NotStarted,
        Briefed,
        Trained,
        Completed
    }
}