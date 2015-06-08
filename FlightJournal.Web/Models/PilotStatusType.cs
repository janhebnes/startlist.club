using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class PilotStatusType
    {
        [Key]
        public int PilotStatusId { get; set; }
        [LocalizedDisplayName("Pilotstatus Name")]
        public string Name { get; set; }
        [LocalizedDisplayName("Description")]
        public string Description { get; set; }
        
        public int? ClubId { get; set; }
        [LocalizedDisplayName("Club")]
        public virtual Club Club { get; set; }
    }
}
