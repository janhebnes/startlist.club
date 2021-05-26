using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;
using Microsoft.Identity.Client;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Training relevant data for a specific flight (and pilot)
    /// 
    /// Apparently, we cannot carry the FlightContext around in EF, hence this Entity wrapper
    /// </summary>
    public class TrainingDataWrapper
    {
        internal TrainingDataWrapper(FlightContext db, int pilotId, int instructorId, Flight flight, int trainingProgramId)
        {
            FlightId = flight.FlightId;
            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId).Select(x => new PilotFlightView{FlightId = x.FlightId, Timestamp = x.Departure ?? x.Date}).OrderBy(v=>v.Timestamp).ToList();
            FlightAnnotations = PilotFlights.SelectMany(x => db.TrainingFlightAnnotations.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Timestamp)).ToList();
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Timestamp)).ToList();
            if (trainingProgramId == -1)
            {  // is the user is participating in exactly one program, use it, otherwise force the user to select
                var tps = AppliedExercises.Select(x => x.Program.Training2ProgramId).Distinct().ToList();
                switch (tps.Count)
                {
                    case 0:
                        trainingProgramId = -1;
                        HasMultipleTrainingPrograms = false;
                        break;
                    case 1:
                        trainingProgramId = tps.First();
                        HasMultipleTrainingPrograms = false;
                        break;
                    default:
                        // pick the tp that was last in use
                        trainingProgramId = AppliedExercises.Last().Program.Training2ProgramId;
                        HasMultipleTrainingPrograms = true;
                        break;
                }
            }
            TrainingProgram = db.TrainingPrograms.SingleOrDefault((x => x.Training2ProgramId == trainingProgramId)); // ?? db.TrainingPrograms.First();
            TrainingPrograms = db.TrainingPrograms.Select(x => new TrainingProgramSelectorViewModel { Name = x.ShortName, Id = x.Training2ProgramId }).ToList();
            
            Manouvres = db.Manouvres.ToList();
            WindDirections = db.WindDirections.ToList();
            WindSpeeds = db.WindSpeeds.ToList();
            Commentaries = db.Commentaries.ToList();
            CommentaryTypes = db.CommentaryTypes.ToList();
            Gradings = db.Gradings.ToList();
            TrainingFlightAnnotationCommentCommentTypes = db.TrainingFlightAnnotationCommentCommentTypes.Include("CommentaryType");

            Instructors = db.Pilots
                .Where(p => p.InstructorId!=null)
                .OrderBy(p => p.Club.ShortName)
                .ThenBy(p => p.Name)
                .ToList();
            InstructorId = instructorId;
        }

        public int InstructorId { get; }

        /// <summary>
        /// The current (latest used) training program for the pilot
        /// </summary>
        public Training2Program TrainingProgram { get; }
        public bool HasMultipleTrainingPrograms { get; }

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
        public IEnumerable<PilotFlightView> PilotFlights { get; }
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
        public IEnumerable<Grading> Gradings { get; }

        public IEnumerable<Pilot> Instructors { get; }

    }

    public class PilotFlightView
    {
        public Guid FlightId { get; set; }
        public DateTime Timestamp { get; set; }
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
        public int InstructorId { get; }
        public IEnumerable<AppliedExerciseViewModel> ExercisesWithStatus { get; } = Enumerable.Empty<AppliedExerciseViewModel>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="date"></param>
        public FlightLogEntryViewModel(Guid flightId, TrainingDataWrapper db, DateTime date)
        {
            Date = date;
            // multiple exercises possible per flight
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flightId).ToList();
            InstructorId =
                exercisesForThisFlight.FirstOrDefault(x => x.Instructor != null)?.Instructor.PilotId
                ?? db.InstructorId;

            ExercisesWithStatus = exercisesForThisFlight.Select(x => new AppliedExerciseViewModel(db, x));
            // zero or one per flight expected
            var annotationsForThisFlight = db.FlightAnnotations.FirstOrDefault(x => x.FlightId == flightId);
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

            FlightLog = dbmodel.PilotFlights.Select(x=>new FlightLogEntryViewModel(x.FlightId, dbmodel, x.Timestamp));

            TrainingProgram = new TrainingProgramViewModel(dbmodel);
            TrainingPrograms = dbmodel.TrainingPrograms;
            Manouvres = dbmodel.Manouvres.OrderBy(x=>x.DisplayOrder);
            GradingsForBriefingPartialExercises = dbmodel.Gradings.Where(x=>x.AppliesToBriefingOnlyPartialExercises && x.Value > 0).OrderBy(x=>x.DisplayOrder);
            GradingsForPracticalPartialExercises = dbmodel.Gradings.Where(x=>x.AppliesToPracticalPartialExercises && x.Value > 0).OrderBy(x=>x.DisplayOrder);
            AnnotationsForFlightPhases =
                dbmodel.CommentaryTypes
                    .OrderBy(c => c.DisplayOrder)
                    .Select(x => new FlightPhaseAnnotationViewModel() {Phase = x, Options = x.Commentaries.OrderBy(y=>y.DisplayOrder)});

            ThisFlight = new FlightLogEntryViewModel(flightId, dbmodel, date);
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

            Instructors = dbmodel.Instructors;
        }


        public Guid FlightId { get; }

        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<FlightLogEntryViewModel> FlightLog { get; } // previous flights
        public TrainingProgramViewModel TrainingProgram;

        // Selectable stuff
        public IEnumerable<Manouvre> Manouvres { get; }
        public IEnumerable<Grading> GradingsForBriefingPartialExercises{ get; }
        public IEnumerable<Grading> GradingsForPracticalPartialExercises { get; }
        public List<WindDirectionViewModel> WindDirections { get; }
        public List<WindSpeedViewModel> WindSpeeds { get; }
        
        public IEnumerable<FlightPhaseAnnotationViewModel> AnnotationsForFlightPhases{ get; }
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }

        public int? AnnotationIdForOk { get; }

        public IEnumerable<Pilot> Instructors { get; }

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
        public int  Id { get; }
        public bool HasMultipleTrainingPrograms { get; }

        public string Name { get; } 
        public IEnumerable<TrainingLessonWithOverallStatusViewModel> Lessons { get; }

        public TrainingProgramViewModel(TrainingDataWrapper db)
        {
            if (db?.TrainingProgram != null)
            {
                HasMultipleTrainingPrograms = db.HasMultipleTrainingPrograms;
                Id = db.TrainingProgram.Training2ProgramId;
                Name = db.TrainingProgram.Name;
                Lessons = db.TrainingProgram.Lessons.Select(less => new TrainingLessonWithOverallStatusViewModel(less, db)).ToList();
            }
            else
            {
                HasMultipleTrainingPrograms = false;
                Id = -1;
                Name = "";
                Lessons = Enumerable.Empty<TrainingLessonWithOverallStatusViewModel>();
            }
        }
    }

    /// <summary>
    /// ViewModel for a lesson (and its exercises) in the context of a pilot
    /// </summary>
    public class TrainingLessonWithOverallStatusViewModel
    {
        public string Id { get; }
        public string ShortName { get; }
        public string Name{ get; }

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
            ShortName = lesson.Name;
            var intro = lesson.Purpose.FirstLine().RemoveNonAlphaNumPrefix().Trim();
            Name = intro.Any() ? $"{lesson.Name}-{intro}" : lesson.Name;
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
                : TrainingStatus.InProgress;
            Debug.WriteLine($"     {StatusSummary} -> {Status}");
        }
    }

    /// <summary>
    /// ViewModel for an exercise in the context of a pilot - used for overview
    /// </summary>
    public class TrainingExerciseWithOverallStatusViewModel
    {
        public string Id { get; }

        public string Description { get; }
        public string LongDescription { get; }

        public TrainingStatus Status { get; }

        public bool BriefingOnlyRequired { get; }
        public int DisplayOrder { get; }
        public bool Regression { get; }
        public Grading BestGrading { get; }
        public Grading GradingInThisFlight {get; }
    
        public TrainingExerciseWithOverallStatusViewModel(Training2Exercise exercise, TrainingDataWrapper db)
        {
            Id = exercise.Training2ExerciseId.ToString();
            Description = exercise.Name;
            LongDescription = exercise.Note;
            BriefingOnlyRequired = exercise.IsBriefingOnly;
            DisplayOrder = exercise.DisplayOrder;

            var totalApplied = db.AppliedExercises.Where(x => x.Exercise == exercise).ToList();
            var appliedInThisFlight = totalApplied.LastOrDefault(x => x.FlightId == db.FlightId); // should be only one
            var thisGrading = appliedInThisFlight?.Grading;
            var flightIdsWithThisExercise = totalApplied.Select(x => x.FlightId).ToList();
            var latestFlightWithThisExercise = db.PilotFlights
                .Where(x => flightIdsWithThisExercise.Contains(x.FlightId)).OrderBy(x => x.Timestamp)
                .LastOrDefault();

            var latestGradingOfThisExercise = totalApplied.LastOrDefault(x => x.FlightId == latestFlightWithThisExercise?.FlightId)?.Grading; // should be only one
            var bestGrading = totalApplied.OrderBy(x => x.Grading?.Value).LastOrDefault()?.Grading;
            GradingInThisFlight = thisGrading != null && thisGrading.Value > 0 ? thisGrading : null;
            BestGrading = bestGrading != null && bestGrading.Value > 0 ? bestGrading : null;
            if (exercise.IsBriefingOnly)
                Regression = false;
            else
                Regression = latestGradingOfThisExercise?.Value < bestGrading?.Value;

            Status = bestGrading == null || bestGrading.Value == 0
                ? TrainingStatus.NotStarted 
                : bestGrading.IsOk 
                    ? TrainingStatus.Completed 
                    : TrainingStatus.InProgress;

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
        //public ExerciseAction Action { get; set; }

        public AppliedExerciseViewModel(TrainingDataWrapper db, AppliedExercise appliedExercise)
        {
            //Action = appliedExercise.Action;
            var program = db.TrainingProgram;
            var lesson = program.Lessons.SingleOrDefault(x=>x == appliedExercise.Lesson);
            var exercise = lesson.Exercises.SingleOrDefault(x => x == appliedExercise.Exercise);
            Description = $"{program?.Name} {lesson?.Name} {exercise?.Name}";
        }
    }


    public enum TrainingStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
}