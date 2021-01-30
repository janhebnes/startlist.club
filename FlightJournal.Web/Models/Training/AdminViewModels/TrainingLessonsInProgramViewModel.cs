using System.Collections.Generic;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training.AdminViewModels
{
    public class TrainingLessonsInProgramViewModel
    {
        public TrainingLessonsInProgramViewModel()
        {
        }

        public TrainingLessonsInProgramViewModel(Training2Program program, IEnumerable<Training2Lesson> lessons)
        {
            TrainingProgramId = program.Training2ProgramId;
            TrainingProgramName = $"{program.ShortName} ({program.Name})";
            Lessons = lessons;
        }
        public IEnumerable<Training2Lesson> Lessons { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
    }
}