using FlightJournal.Web.Models.Training.Flight;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public bool IsOk{ get; set; }

        public bool AppliesToStartPhase => CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("start"));
        public bool AppliesToFlightPhase => CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("flight"));
        public bool AppliesToApproachPhase => CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("approach"));
        public bool AppliesToLandingPhase => CommentaryTypes.ToList().Any(x => x.CType.ToLower().Equals("landing"));
        public virtual ICollection<CommentaryType> CommentaryTypes { get; set; }
        public virtual ICollection<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; }
    }
}