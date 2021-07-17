using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;
using Newtonsoft.Json;

namespace FlightJournal.Web.FlightExport
{
    // formats flights as string
    public enum FlightExportFormat
    {
        CSV,
        JSON
    }

    public interface ITrainingFlightExportFormatter
    { 
        string Format(IEnumerable<Flight> flights);
    }

    


    public class JsonTrainingFlightExportFormatter : ITrainingFlightExportFormatter
    {
        private readonly IFlightExportModelCreator _modelCreator;

        public JsonTrainingFlightExportFormatter(IFlightExportModelCreator modelCreator)
        {
            _modelCreator = modelCreator;
        }
        public string Format(IEnumerable<Flight> flights)
        {
            var model = _modelCreator.CreateTrainingFlightsExportModel(flights);
            return JsonConvert.SerializeObject(model, Formatting.Indented);
        }
    }


    public class CsvTrainingFlightExportFormatter : ITrainingFlightExportFormatter
    {
        private readonly IFlightExportModelCreator _modelCreator;

        public CsvTrainingFlightExportFormatter(IFlightExportModelCreator modelCreator)
        {
            _modelCreator = modelCreator;
        }
        public string Format(IEnumerable<Flight> flights)
        {
            var model = _modelCreator.CreateTrainingFlightsExportModel(flights);

            var sb = new StringBuilder();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
            using (var writer = new StringWriter(sb))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(model.Flights);
            }

            return sb.ToString();
        }
    }


    public class FlightExportFormatterCreatorFactory
    {
        public static ITrainingFlightExportFormatter Create(FlightContext db, FlightExportFormat format)
        {
            switch (format)
            {
                case FlightExportFormat.CSV:
                    return new CsvTrainingFlightExportFormatter(new TrainingFlightExportModelCreator(db));
                case FlightExportFormat.JSON:
                    return new JsonTrainingFlightExportFormatter(new TrainingFlightExportModelCreator(db));
                default:
                    Log.Error($"{nameof(FlightExportFormatterCreatorFactory)} unable to create IFlightExportModelCreator of type {format}");
                    return null;
            }
        }
    }


}
