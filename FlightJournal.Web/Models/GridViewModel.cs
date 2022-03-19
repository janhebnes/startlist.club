using System.Collections.Generic;

namespace FlightJournal.Web.Models
{
    public class GridViewModel
    {
        public IEnumerable<Flight> Flights { get; }
        public Dictionary<int, TrainingBarometerViewModel> TrainingBarometers { get; }

        public GridViewModel(IEnumerable<Flight> flights,
            Dictionary<int, TrainingBarometerViewModel> trainingBarometers)
        {
            Flights = flights;
            TrainingBarometers = trainingBarometers;
        }
    }
}