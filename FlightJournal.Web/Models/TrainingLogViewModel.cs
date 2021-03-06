using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Training relevant data for a specific flight (and pilot)
    /// 
    /// Apparently, we cannot carry the FlightContext around in EF, hence this Entity wrapper
    /// </summary>
    public class TrainingDataWrapper
    {
        internal TrainingDataWrapper(FlightContext db, int pilotId, Flight flight, int trainingProgramId)
        {
            FlightId = flight.FlightId;

            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId).OrderBy(x => x.Landing ?? x.Date);
            FlightAnnotations = PilotFlights.SelectMany(x => db.TrainingFlightAnnotations.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            TrainingProgram = db.TrainingPrograms.SingleOrDefault((x => x.Training2ProgramId == trainingProgramId)) ?? db.TrainingPrograms.First();
            TrainingPrograms = db.TrainingPrograms.Select(x => new TrainingProgramSelectorViewModel { Name = x.ShortName, Id = x.Training2ProgramId }).ToList();
            
            Manouvres = db.Manouvres.ToList();
            WindDirections = db.WindDirections.ToList();
            WindSpeeds = db.WindSpeeds.ToList();
            Commentaries = db.Commentaries.ToList();
            CommentaryTypes = db.CommentaryTypes.ToList();
            TrainingFlightAnnotationCommentCommentTypes = db.TrainingFlightAnnotationCommentCommentTypes.Include("CommentaryType");
            
        }

        /// <summary>
        /// The current (latest used) training program for the pilot
        /// </summary>
        public Training2Program TrainingProgram { get; }

        /// <summary>
        /// All available training programs
        /// </summary>
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }


        // flight specific data

        /// <summary>
        /// This flight
        /// </summary>
        public Guid FlightId { get; }

        /// <summary>
        /// All flights by the pilot
        /// </summary>
        public IEnumerable<Flight> PilotFlights { get; }
        /// <summary>
        /// Exercises flown across all flights by this pilot
        /// </summary>
        public IEnumerable<AppliedExercise> AppliedExercises { get; }
        /// <summary>
        /// Annotations (gradings/evaluations/comments) across all flights by this pilot
        /// </summary>
        public IEnumerable<TrainingFlightAnnotation> FlightAnnotations{ get; } // across all PilotFlights
        public IEnumerable<Manouvre> Manouvres { get; } // getting manouvres from the db
        public IEnumerable<WindSpeed> WindSpeeds {get;}
        public IEnumerable<WindDirection> WindDirections { get; }
        public IEnumerable<Commentary> Commentaries { get; }
        public IEnumerable<CommentaryType> CommentaryTypes { get; }
        public IEnumerable<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; }


    }


    public class TrainingProgramSelectorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TrainingProgramSelectorViewModel(){}

    }
    
    /// <summary>
    /// Viewmodel for an actual training flight
    /// </summary>
    public class FlightLogEntryViewModel
    {
        public DateTime Date { get; }
        public string Notes { get; }
        public List<int> Manouvres { get; } = new List<int>();
        public Dictionary<int, IEnumerable<int>> CommentIdsByPhase { get; } = new Dictionary<int, IEnumerable<int>>();
        public int WindSpeed { get; }
        public int WindDirection { get; }

        public IEnumerable<AppliedExerciseViewModel> ExercisesWithStatus { get; } = Enumerable.Empty<AppliedExerciseViewModel>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="date"></param>
        public FlightLogEntryViewModel(Flight flight, TrainingDataWrapper db, DateTime date)
        {
            Date = date;
            // multiple exercises possible per flight
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flight.FlightId).ToList();
            ExercisesWithStatus = exercisesForThisFlight.Select(x => new AppliedExerciseViewModel(db, x));
            // zero or one per flight expected
            var annotationsForThisFlight = db.FlightAnnotations.Where(x => x.FlightId == flight.FlightId).FirstOrDefault();
            if (annotationsForThisFlight != null) {
                Notes = string.Join("; ", annotationsForThisFlight.Note);
                WindSpeed = annotationsForThisFlight.WindSpeed;
                WindDirection = annotationsForThisFlight.WindDirection;
                Manouvres = annotationsForThisFlight.Manouvres?.Select(m => m.ManouvreId).ToList() ?? new List<int>();
                CommentIdsByPhase = annotationsForThisFlight
                    .TrainingFlightAnnotationCommentCommentTypes?
                    .GroupBy(e => e.CommentaryType.CommentaryTypeId, e=>e.CommentaryId, (phaseId, commentIds) => new{phaseId, commentIds})
                    .ToDictionary(
                        x => x.phaseId, 
                        x=> x.commentIds) 
                                    ?? new Dictionary<int, IEnumerable<int>>();
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

    /// <summary>
    /// ViewModel for items used to describe a training flight.
    ///
    /// The actual input from the instructor ends up in a FlightLogEntryViewModel
    /// </summary>
    public class TrainingLogViewModel
    {
        public TrainingLogViewModel(Guid flightId, DateTime date, DateTime? started, DateTime? landed, string pilot, string backseatPilot, TrainingDataWrapper dbmodel)
        {
            FlightId = flightId;
            TimeInfo = $"{date.ToShortDateString()} ({started?.ToShortTimeString()} - {landed?.ToShortTimeString()})";
            Pilot = pilot;
            BackseatPilot = backseatPilot;

            FlightLog = dbmodel.PilotFlights.Select(x=>new FlightLogEntryViewModel(x, dbmodel, x.Date));

            TrainingProgram = new TrainingProgramViewModel(dbmodel.TrainingProgram, dbmodel);
            TrainingPrograms = dbmodel.TrainingPrograms;
            Manouvres = dbmodel.Manouvres;
            AnnotationsForFlightPhases =
                dbmodel.CommentaryTypes
                    .OrderBy(c => c.DisplayOrder)
                    .Select(x => new FlightPhaseAnnotationViewModel() {Phase = x, Options = x.Commentaries});

            ThisFlight = new FlightLogEntryViewModel(dbmodel.PilotFlights.Single(x => x.FlightId == dbmodel.FlightId), dbmodel, date);
            WindDirections = dbmodel.WindDirections.Select(wd => new WindDirectionViewModel(wd.WindDirectionItem)).ToList();
            if (!WindDirections.Exists(x => x.Value == ThisFlight.WindDirection))
            {
                WindDirections.Add(new WindDirectionViewModel(ThisFlight.WindDirection));
            }
            WindDirections = WindDirections.OrderBy(x => x.Value).ToList();
            
            WindSpeeds = dbmodel.WindSpeeds.Select(ws => new WindSpeedViewModel(ws.WindSpeedItem)).ToList();
            if(!WindSpeeds.Exists(x=>x.Value == ThisFlight.WindSpeed))
            {
                WindSpeeds.Add(new WindSpeedViewModel(ThisFlight.WindSpeed));
            }
            WindSpeeds = WindSpeeds.OrderBy(x => x.Value).ToList();
            AnnotationIdForOk = dbmodel.Commentaries.FirstOrDefault(x => x.IsOk)?.CommentaryId;
        }


        public Guid FlightId { get; }

        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<FlightLogEntryViewModel> FlightLog { get; } // previous flights
        public TrainingProgramViewModel TrainingProgram;

        // Selectable stuff
        public IEnumerable<Manouvre> Manouvres { get; }
        public List<WindDirectionViewModel> WindDirections { get; }
        public List<WindSpeedViewModel> WindSpeeds { get; }
        
        public IEnumerable<FlightPhaseAnnotationViewModel> AnnotationsForFlightPhases{ get; }
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }

        public int? AnnotationIdForOk { get; }

        // data for this flight
        public FlightLogEntryViewModel ThisFlight { get; }
        public string TimeInfo { get; }
    }

    public class  FlightPhaseAnnotationViewModel{
        public CommentaryType Phase{ get; set; } 
        public IEnumerable<Commentary> Options { get; set; }
    }

/// <summary>
/// Viewmodel for a Training program (with lessons -> exercises).
/// Used for building UI selections.
/// </summary>
public class TrainingProgramViewModel
    {
        public string Id { get; }

        public string Name { get; } 
        public IEnumerable<TrainingLessonWithOverallStatusViewModel> Lessons { get; }

        public TrainingProgramViewModel(Training2Program program, TrainingDataWrapper db)
        {
            Id = program.Training2ProgramId.ToString();
            Name = program.Name;
            Lessons = program.Lessons.Select(less => new TrainingLessonWithOverallStatusViewModel(less, db)).ToList();
        }
    }

    /// <summary>
    /// ViewModel for a lesson (and its exercises) in the context of a pilot
    /// </summary>
    public class TrainingLessonWithOverallStatusViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public string Description { get; }
        public string Precondition { get; }

        public IEnumerable<TrainingExerciseWithOverallStatusViewModel> Exercises { get; }

        public int ExercisesTotal { get; }
        /// <summary>
        /// Exercises completed by this pilot
        /// </summary>
        public int ExercisesCompleted { get; }
        /// <summary>
        /// Exercises in progress by this pilot
        /// </summary>
        public int ExercisesInProgress { get; }
        /// <summary>
        /// Exercises not yet started by this pilot
        /// </summary>
        public int ExercisesNotStarted { get; }

        public string StatusSummary => $"{ExercisesNotStarted}/{ExercisesInProgress}/{ExercisesCompleted} ({ExercisesTotal})";

        /// <summary>
        /// Overall status of the Lesson
        /// </summary>
        public TrainingStatus Status { get; }

        public int DisplayOrder { get;  }

        public TrainingLessonWithOverallStatusViewModel(Training2Lesson lesson, TrainingDataWrapper db)
        {
            Id = lesson.Training2LessonId.ToString();
            Name = lesson.Name;
            Description = lesson.Purpose;
            Precondition = lesson.Precondition;
            DisplayOrder = lesson.DisplayOrder;
            Exercises = lesson.Exercises.Select(ex => new TrainingExerciseWithOverallStatusViewModel(ex, db)).ToList();
            ExercisesTotal = Exercises.Count();
            ExercisesCompleted = Exercises.Count(x => x.Status == TrainingStatus.Completed);
            ExercisesNotStarted = Exercises.Count(x => x.Status == TrainingStatus.NotStarted);
            ExercisesInProgress = ExercisesTotal - ExercisesCompleted - ExercisesNotStarted;
            Status = ExercisesTotal == ExercisesCompleted ? TrainingStatus.Completed 
                : ExercisesTotal == ExercisesNotStarted ? TrainingStatus.NotStarted 
                : TrainingStatus.Trained;
            Debug.WriteLine($"     {StatusSummary} -> {Status}");
        }
    }

    /// <summary>
    /// ViewModel for an exercise in the context of a pilot - used for overview
    /// </summary>
    public class TrainingExerciseWithOverallStatusViewModel
    {
        public enum CheckBoxType{
            PlaceHolder,
            DisabledUnchecked,
            DisabledChecked,
            EnabledUnchecked,
            EnabledChecked
        };

        public string Id { get; }

        public string Description { get; }
        public string LongDescription { get; }

        public TrainingStatus Status { get; }

        public ExerciseAction ActionInThisFlight { get; set; }

        public bool IsBriefed => Status == TrainingStatus.Briefed
                                 || Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;

        public bool IsBriefedInThisFlight => ActionInThisFlight == ExerciseAction.Briefed
                                           || ActionInThisFlight == ExerciseAction.Trained
                                           || ActionInThisFlight == ExerciseAction.Completed;


        public bool IsTrained => Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;

        public bool IsTrainedInThisFlight => ActionInThisFlight == ExerciseAction.Trained
                                             || ActionInThisFlight == ExerciseAction.Completed;

        public bool IsCompleted => Status == TrainingStatus.Completed;
        public bool IsCompletedInThisFlight => ActionInThisFlight == ExerciseAction.Completed;
        public bool BriefingOnlyRequired { get; }
        public int DisplayOrder { get; }
        public bool Regression { get; }
        public CheckBoxType CheckBoxForOverallBriefed =>  IsBriefed ? CheckBoxType.DisabledChecked : CheckBoxType.DisabledUnchecked;
        public CheckBoxType CheckBoxForOverallTrained => BriefingOnlyRequired ? CheckBoxType.PlaceHolder :  IsTrained ? CheckBoxType.DisabledChecked : CheckBoxType.DisabledUnchecked;
        public CheckBoxType CheckBoxForOverallCompleted => BriefingOnlyRequired ? CheckBoxType.PlaceHolder : IsCompleted ? CheckBoxType.DisabledChecked : CheckBoxType.DisabledUnchecked;
        public CheckBoxType CheckBoxForThisFlightBriefed =>  IsBriefedInThisFlight ? CheckBoxType.EnabledChecked : CheckBoxType.EnabledUnchecked;
        public CheckBoxType CheckBoxForThisFlightTrained => BriefingOnlyRequired ? CheckBoxType.PlaceHolder : IsTrainedInThisFlight ? CheckBoxType.EnabledChecked : CheckBoxType.EnabledUnchecked;
        public CheckBoxType CheckBoxForThisFlightCompleted => BriefingOnlyRequired ? CheckBoxType.PlaceHolder : IsCompletedInThisFlight ? CheckBoxType.EnabledChecked : CheckBoxType.EnabledUnchecked;

        public TrainingExerciseWithOverallStatusViewModel(Training2Exercise exercise, TrainingDataWrapper db)
        {
            Id = exercise.Training2ExerciseId.ToString();
            Description = exercise.Name;
            LongDescription = exercise.Note;
            BriefingOnlyRequired = exercise.IsBriefingOnly;
            DisplayOrder = exercise.DisplayOrder;

            var totalApplied = db.AppliedExercises.Where(x => x.Exercise == exercise).ToList();
            var flightsWhereThisExerciseWasAtLeastBriefed = totalApplied.Where(x => x.Action == ExerciseAction.Briefed).Select(f=>f.FlightId);
            var flightsWhereThisExerciseWasAtLeastTrained = totalApplied.Where(x => x.Action == ExerciseAction.Trained).Select(f => f.FlightId);
            var flightsWhereThisExerciseWasCompleted = totalApplied.Where(x => x.Action == ExerciseAction.Completed).Select(f => f.FlightId);
            // check if we have a Trained or Briefed later than a Completed => regression, which should be highlighted.
            var latestBriefedFlight = db.PilotFlights.LastOrDefault(f => flightsWhereThisExerciseWasAtLeastBriefed.Contains(f.FlightId));
            var latestTrainedFlight = db.PilotFlights.LastOrDefault(f => flightsWhereThisExerciseWasAtLeastTrained.Contains(f.FlightId));
            var latestCompletedFlight = db.PilotFlights.LastOrDefault(f => flightsWhereThisExerciseWasCompleted.Contains(f.FlightId));
            Regression = latestCompletedFlight != null 
                         && (LandingTimeOf(latestTrainedFlight) > LandingTimeOf(latestCompletedFlight) 
                         || LandingTimeOf(latestBriefedFlight) > LandingTimeOf(latestCompletedFlight));

            Status =
                flightsWhereThisExerciseWasCompleted.Any() ? TrainingStatus.Completed :
                flightsWhereThisExerciseWasAtLeastTrained.Any() ? TrainingStatus.Trained :
                flightsWhereThisExerciseWasAtLeastBriefed.Any() ? TrainingStatus.Briefed :
                TrainingStatus.NotStarted;

            var appliedInThisFlight = totalApplied.FirstOrDefault(x => x.FlightId == db.FlightId); // should be only one
            ActionInThisFlight = appliedInThisFlight?.Action ?? ExerciseAction.None;

            if (BriefingOnlyRequired && (Status == TrainingStatus.Briefed || Status == TrainingStatus.Trained))
                Status = TrainingStatus.Completed;

        }


        private DateTime LandingTimeOf(Flight flight)
        {
            return flight?.Landing ?? DateTime.MinValue;
        }
    }

    /// <summary>
    /// ViewModel for a flown exercise 
    /// </summary>
    public class AppliedExerciseViewModel
    {
        public string Description { get; }

        /// <summary>
        /// STatus of the flown exercise
        /// </summary>
        public ExerciseAction Action { get; set; }

        public AppliedExerciseViewModel(TrainingDataWrapper db, AppliedExercise appliedExercise)
        {
            Action = appliedExercise.Action;
            var program = db.TrainingProgram;
            var lesson = program.Lessons.SingleOrDefault(x=>x == appliedExercise.Lesson);
            var exercise = lesson.Exercises.SingleOrDefault(x => x == appliedExercise.Exercise);
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