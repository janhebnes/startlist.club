using System.Collections.Generic;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class CommentsInFlightPhasesViewModel
    {
        public IEnumerable<string> FlightPhaseNames{ get; set; }
        public Dictionary<string, IEnumerable<CommentInFlightPhaseViewModel>> Comments{ get; set; }
    }


    public class CommentInFlightPhaseViewModel
    {
        public string  Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool UsedInPhase { get; set; }
    }
}