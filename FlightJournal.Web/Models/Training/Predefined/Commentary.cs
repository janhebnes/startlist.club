using FlightJournal.Web.Models.Training.Flight;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Commentary  //TODO: rename to FlightPhaseComment ?
    {
        [Key]
        public int CommentaryId { get; set; }
        [Required]
        [AllowHtml]
        public string Comment { get; set; }
        public bool IsOk{ get; set; }
        public virtual ICollection<CommentaryType> CommentaryTypes { get; set; } // TODO: rename to FlightPhases (these are the flight phases that this commentapplies to)
        public virtual ICollection<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; } //TODO: rename ?
    }
}