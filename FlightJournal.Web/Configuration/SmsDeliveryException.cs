using System;
using Twilio.Exceptions;

namespace FlightJournal.Web.Configuration
{
    /// <summary>SMS could not be sent. The message carries only codes and modes: never the phone number, the body or Twilio's text.</summary>
    public sealed class SmsDeliveryException : Exception
    {
        public SmsDeliveryException(int? twilioCode, int? twilioStatus, TwilioSmsMode mode, string region, Exception inner = null)
            : base($"SMS delivery failed (twilioCode={twilioCode?.ToString() ?? "-"}, status={twilioStatus?.ToString() ?? "-"}, mode={mode}, region={region ?? "us1"})", inner)
        {
            TwilioCode = twilioCode; TwilioStatus = twilioStatus; Mode = mode; Region = region;
        }

        public int? TwilioCode { get; }
        public int? TwilioStatus { get; }
        public TwilioSmsMode Mode { get; }
        public string Region { get; }

        /// <summary>
        /// Keeps only Twilio's code and status. The ApiException itself is not kept as the inner exception:
        /// its message can contain the From number, and the code and status are enough to diagnose.
        /// </summary>
        public static SmsDeliveryException From(ApiException ex, TwilioSmsConfig config)
            => new SmsDeliveryException(ex.Code, ex.Status, config.Mode, config.Region, inner: null);

        /// <summary>
        /// Other Twilio failures (connection, timeout, authentication) carry no Twilio code or status.
        /// As above, the Twilio exception and its text are not kept.
        /// </summary>
        public static SmsDeliveryException From(TwilioException ex, TwilioSmsConfig config)
            => new SmsDeliveryException(null, null, config.Mode, config.Region, inner: null);
    }
}
