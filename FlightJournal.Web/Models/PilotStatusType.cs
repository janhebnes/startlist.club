using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models
{
    public class PilotStatusType
    {
        [Key]
        public int PilotStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }
    }
}
