using System.Collections.Generic;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class CommentsInFlightPhasesViewModel
    {
        public IEnumerable<CommentaryType> FlightPhases{ get; set; }
        public Dictionary<int, IEnumerable<CommentInFlightPhaseViewModel>> Comments{ get; set; }
    }


    public class CommentInFlightPhaseViewModel
    {
        public int CommentId { get; set; }
        public string  Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool UsedInPhase { get; set; }
    }
}