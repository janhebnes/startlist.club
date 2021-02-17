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

        public virtual ICollection<Commentary> StartAnnotation { get; set; } = new List<Commentary>();
        public virtual ICollection<Commentary> FlightAnnotation { get; set; } = new List<Commentary>(); // applies to the flight in general, as well as the maneuvers
        public virtual ICollection<Commentary> ApproachAnnotation { get; set; } = new List<Commentary>();
        public virtual ICollection<Commentary> LandingAnnotation { get; set; } = new List<Commentary>();

        public virtual Weather Weather { get; set; }
        public virtual ICollection<Manouvre> Manouvres { get; set; }
        public ICollection<TrainingFlightAnnotationCommentCommentType> TrainingFlightAnnotationCommentCommentTypes { get; set; }
    }
}