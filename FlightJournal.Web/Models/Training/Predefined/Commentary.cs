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
        public virtual ICollection<CommentaryType> CommentaryTypes { get; set; }
        public virtual ICollection<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; }
    }
}