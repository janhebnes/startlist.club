using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models
{
    public class InstructorPilotRelation
    {
        [Required]
        public int InstructorId { get; set; }
        [Required]
        public int PilotId { get; set; }
    }
}