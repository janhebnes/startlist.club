using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models.Training.Flight
{
    public class TrainingFlightAnnotationCommentCommentType
    {
        public int TrainingFlightAnnotationId { get; set; }
        public int CommentaryId { get; set; }
        public int CommentaryTypeId { get; set; }

        public TrainingFlightAnnotation TrainingFlightAnnotation { get; set; }
        public Commentary Commentary { get; set; } 
        public CommentaryType CommentaryType { get; set; }
    }
}