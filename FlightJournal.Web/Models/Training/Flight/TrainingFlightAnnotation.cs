using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models.Training.Flight
{
    /// <summary>
    /// Annotations for a training flight that do not directly refer to a Training2Exercise.
    /// Textual note (by instructor)
    /// SFIL classification./// 
    /// Wind (direction, strength)
    /// 
    /// </summary>
    public class TrainingFlightAnnotation
    {
        [Key]
        public int TrainingFlightAnnotationId { get; set; }
        [Required]
        public Guid FlightId{ get; set; } 
        public string Note { get; set; }
        public virtual Weather Weather { get; set; }
        public virtual ICollection<Manouvre> Manouvres { get; set; } = new HashSet<Manouvre>();
        public virtual ICollection<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; } = new HashSet<TrainingFlightAnnotationCommentCommentType>();
    }
}