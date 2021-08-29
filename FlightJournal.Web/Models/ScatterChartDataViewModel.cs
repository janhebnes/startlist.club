using System.Collections.Generic;

namespace FlightJournal.Web.Models
{
    public class ScatterChartDataViewModel
    {
        public ScatterChartDataViewModel(IEnumerable<TimestampedDataSeriesViewModel> dataSeries)
        {
            DataSeries = dataSeries;
        }

        public ScatterChartDataViewModel(TimestampedDataSeriesViewModel dataSeries) : this(new[] { dataSeries})
        {
        }

        public string YLabel { get; set; }
        public string XLabel { get; set; }

        public IEnumerable<TimestampedDataSeriesViewModel> DataSeries { get; set; }
        public string MaxHeight { get; set; } = "400px";
        public Dictionary<string, string> ValueLabels { get; set; }
        public Dictionary<string,string> Metadata { get; set; }

    }
}