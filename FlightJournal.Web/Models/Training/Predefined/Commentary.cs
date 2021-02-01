using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Commentary
    {
        [Key]
        public int CommentaryId { get; set; }
        [Required]
        [AllowHtml]
        public string Comment { get; set; }

        public bool AppliesToStartPhase { get; set; }
        public bool AppliesToFlightPhase { get; set; }
        public bool AppliesToApproachPhase { get; set; }
        public bool AppliesToLandingPhase { get; set; }

        public int CommentaryTypeId { get; set; }
        public virtual ICollection<CommentaryType> CommentaryTypes { get; set; }
    }
}