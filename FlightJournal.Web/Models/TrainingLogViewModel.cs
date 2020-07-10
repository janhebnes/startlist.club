using System.Collections.Generic;
using System.Linq;

namespace FlightJournal.Web.Models
{
    public class TrainingLogViewModel
    {
        public IEnumerable<TrainingProgramViewModel> TrainingPrograms { get; set; } = new List<TrainingProgramViewModel>();
    }

    public class TrainingProgramViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<TrainingLessonViewModel> Lessons { get; set; } = new List<TrainingLessonViewModel>();

    }

    public class TrainingLessonViewModel
    {
        public string Id { get; set; }
        public string Name { set; get; }

        public string Description { set; get; }
        public IEnumerable<TrainingExerciseViewModel> Exercises { get; set; } = new List<TrainingExerciseViewModel>();

        public TrainingStatus Status => //TrainingStatus.NotStarted;
        Exercises.All(x => x.Status == TrainingStatus.NotStarted)
            ? TrainingStatus.NotStarted
            : Exercises.All(x => x.Status == TrainingStatus.Completed)
                ? TrainingStatus.Completed
                : TrainingStatus.Trained;
    }

    public class TrainingExerciseViewModel
    {
        public string Id { get; set; }

        public string Description { set; get; }
        public string LongDescription { set; get; }
        public TrainingStatus Status { get; set; } = TrainingStatus.NotStarted;

        public bool IsBriefed => Status == TrainingStatus.Briefed 
                                 || Status == TrainingStatus.Trained 
                                 || Status == TrainingStatus.Completed;
        public bool IsTrained => Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;
        public bool IsCompleted => Status == TrainingStatus.Completed;
        public bool BriefingOnlyRequired { get; set; } = false;
    }

    public enum TrainingStatus
    {
        NotStarted,
        Briefed,
        Trained,
        Completed
    }
}