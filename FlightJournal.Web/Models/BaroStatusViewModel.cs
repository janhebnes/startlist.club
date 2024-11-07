using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightJournal.Web.Models
{
    public class BaroStatusViewModel
    {
        public IEnumerable<PilotTrainingBarometerViewModel> Pilots { get; set; }
    }

    public class PilotTrainingBarometerViewModel
    {
        public Pilot Pilot { get; set; }
        public TrainingBarometerViewModel TrainingStatus { get; set; }
    }
}