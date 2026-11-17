using System;
using Twilio.Clients;

namespace FlightJournal.Web.Configuration
{
    public enum TwilioSmsMode { NotConfigured, Misconfigured, LegacyUs1AuthToken, RegionalApiKey }

    /// <summary>
    /// How the app talks to Twilio for SMS. Pure: built from settings values, no I/O.
    /// Credentials are only valid in the Twilio Region they were created in, and a region without an edge routes to US1.
    /// </summary>
    public sealed class TwilioSmsConfig
    {
        private TwilioSmsConfig(TwilioSmsMode mode, string username, string password, string accountSid,
                                string region, string edge, string fromNumber, string problem)
        {
            Mode = mode; Username = username; Password = password; AccountSid = accountSid;
            Region = region; Edge = edge; FromNumber = fromNumber; Problem = problem;
        }

        public TwilioSmsMode Mode { get; }
        public string Username { get; }   // API key SID (SK...) or, legacy, the account SID
        public string Password { get; }   // API key secret or, legacy, the auth token. Never logged.
        public string AccountSid { get; }
        public string Region { get; }     // null = US1
        public string Edge { get; }
        public string FromNumber { get; }
        public string Problem { get; }    // why Misconfigured; setting names only, never values

        /// <summary>
        /// True only when every required setting is present (regional API key or legacy auth token).
        /// The single rule for whether SMS may be offered or attempted; blank or whitespace in any required field means false.
        /// </summary>
        public bool CanSend => Mode == TwilioSmsMode.RegionalApiKey || Mode == TwilioSmsMode.LegacyUs1AuthToken;

        public static TwilioSmsConfig Resolve(string accountSid, string authToken, string fromNumber,
                                              string region, string edge, string apiKeySid, string apiKeySecret)
        {
            if (IsBlank(accountSid) || IsBlank(fromNumber))
                return NotConfigured();

            var hasKey = !IsBlank(apiKeySid) || !IsBlank(apiKeySecret);
            if (!IsBlank(region))
            {
                if (IsBlank(apiKeySid) || IsBlank(apiKeySecret))
                    return Misconfigured("TwilioRegion is set, but TwilioApiKeySid or TwilioApiKeySecret is missing");
                var r = region.Trim().ToLowerInvariant();
                var e = IsBlank(edge) ? DefaultEdge(r) : edge.Trim().ToLowerInvariant();
                if (e == null)
                    return Misconfigured("TwilioEdge is required for region " + r);
                return new TwilioSmsConfig(TwilioSmsMode.RegionalApiKey, apiKeySid.Trim(), apiKeySecret, accountSid.Trim(),
                                           r, e, fromNumber.Trim(), null);
            }
            if (hasKey)
                return Misconfigured("TwilioApiKeySid/TwilioApiKeySecret are set without TwilioRegion");
            if (IsBlank(authToken))
                return NotConfigured();
            return new TwilioSmsConfig(TwilioSmsMode.LegacyUs1AuthToken, accountSid.Trim(), authToken, accountSid.Trim(),
                                       null, null, fromNumber.Trim(), null);
        }

        /// <summary>Only for Modes that send; no network until a request is made.</summary>
        public TwilioRestClient CreateClient()
        {
            if (!CanSend)
                throw new InvalidOperationException("Twilio SMS is " + Mode);
            // Positional, in the documented order: (username, password, accountSid, region, httpClient, edge)
            return new TwilioRestClient(Username, Password, AccountSid, Region, null, Edge);
        }

        private static string DefaultEdge(string region) => region == "ie1" ? "dublin" : null;
        private static bool IsBlank(string s) => string.IsNullOrWhiteSpace(s);
        private static TwilioSmsConfig NotConfigured() => new TwilioSmsConfig(TwilioSmsMode.NotConfigured, null, null, null, null, null, null, null);
        private static TwilioSmsConfig Misconfigured(string problem) => new TwilioSmsConfig(TwilioSmsMode.Misconfigured, null, null, null, null, null, null, problem);
    }
}
