using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training.AdminViewModels
{
    public class TrainingLessonInProgramViewModel
    {
        public TrainingLessonInProgramViewModel()
        {
        }

        public TrainingLessonInProgramViewModel(Training2Program program, Training2Lesson lesson)
        {
            TrainingProgramId = program.Training2ProgramId;
            TrainingProgramName = $"{program.ShortName} ({program.Name})";
            Lesson = lesson;
        }
        public Training2Lesson Lesson { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
    }
}