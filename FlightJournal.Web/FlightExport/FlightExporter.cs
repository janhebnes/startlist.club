using System.Collections.Generic;
using FlightJournal.Web.Logging;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.FlightExport
{
    /// <summary>
    /// Exports flights to external destination
    /// </summary>

    public class FlightExporter
    {        
        public enum ExportResult
        {
            SUCCESS,
            AUTH_FAILURE,
            OTHER_FAILURE
        };


        private readonly ITrainingFlightExportFormatter _formatter;
        private readonly IFlightExporterAuthenticator _authenticator;
        private readonly IFlightExporterUploader _uploader;

        public FlightExporter(ITrainingFlightExportFormatter formatter, IFlightExporterAuthenticator authenticator, IFlightExporterUploader uploader)
        {
            _formatter = formatter;
            _authenticator = authenticator;
            _uploader = uploader;
        }


        public ExportResult Export(IEnumerable<Flight> flights)
        {
            var formatted = _formatter.Format(flights);
            if (_uploader.Upload(formatted))
                return ExportResult.SUCCESS;

            if(!_authenticator.Authenticate())
                return ExportResult.AUTH_FAILURE;
            // now authenticated, pass along and retry
            _uploader.SetAuthInfoFrom(_authenticator);
            if (_uploader.Upload(formatted))
                return ExportResult.SUCCESS;

            return ExportResult.OTHER_FAILURE;
        }
    }




    public class FlightExporterFactory
    {
        public enum FlightExporterType
        {
            DSvU,
            NoAuthenticationJsonPost,
            NoAuthenticationCsvPost
        }

        public static FlightExporter Create(FlightContext db, ExportRecipient recipient)
        {
            switch (recipient.ExporterType)
            {
                case FlightExporterType.DSvU:
                    return new FlightExporter(
                        FlightExportFormatterCreatorFactory.Create(db, FlightExportFormat.JSON),
                        new FormUrlEncodedTokenDeliveringFlightExporterAuthenticator(recipient.AuthenticationUrl, recipient.Username, recipient.Password),
                        new BearerAuthUploader(recipient.DeliveryUrl, "application/json"));

                case FlightExporterType.NoAuthenticationJsonPost:
                    return new FlightExporter(
                        FlightExportFormatterCreatorFactory.Create(db, FlightExportFormat.JSON),
                        new DummyFlightExporterAuthenticator(),
                        new BaseUploader(recipient.DeliveryUrl, "application/json"));

                case FlightExporterType.NoAuthenticationCsvPost:
                    return new FlightExporter(
                        FlightExportFormatterCreatorFactory.Create(db, FlightExportFormat.CSV),
                        new DummyFlightExporterAuthenticator(),
                        new BaseUploader(recipient.DeliveryUrl, "text/csv"));

                default:
                    Log.Error($"{nameof(FlightExporterFactory)} unable to create FlightExporter of type {recipient.ExporterType}");
                    return null;
            }
        }
    }
}
