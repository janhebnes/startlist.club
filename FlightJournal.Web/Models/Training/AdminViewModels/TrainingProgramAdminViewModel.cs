using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training.AdminViewModels
{
    public class TrainingProgramAdminViewModel
    {
        public TrainingProgramAdminViewModel()
        {
        }
        public TrainingProgramAdminViewModel(Training2Program p)
        {
            Id = p.Training2ProgramId;
            Name = p.Name;
            ShortName = p.ShortName;
            Notes = p.Notes;
            Url = p.Url;
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Notes { get; set; }
        public string Url { get; set; }
    }
}