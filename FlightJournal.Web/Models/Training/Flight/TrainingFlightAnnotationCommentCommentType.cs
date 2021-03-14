using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models.Training.Flight
{
    public class TrainingFlightAnnotationCommentCommentType // TODO: rename according to renamed references ?
    {
        public int TrainingFlightAnnotationId { get; set; }
        public int CommentaryId { get; set; }
        public int CommentaryTypeId { get; set; }

        public virtual TrainingFlightAnnotation TrainingFlightAnnotation { get; set; }
        public virtual Commentary Commentary { get; set; } 
        public virtual CommentaryType CommentaryType { get; set; }
    }
}