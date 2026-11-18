using System;
using System.Collections.Generic;
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
                                string region, string edge, string fromNumber, string problem,
                                IReadOnlyList<string> missing = null)
        {
            Mode = mode; Username = username; Password = password; AccountSid = accountSid;
            Region = region; Edge = edge; FromNumber = fromNumber; Problem = problem;
            Missing = missing ?? new string[0];
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
        /// Names (never values) of the required settings that are blank, for the mode being set up; empty when SMS can send.
        /// Regional when any regional setting (TwilioRegion, TwilioEdge, TwilioApiKeySid, TwilioApiKeySecret) is set;
        /// legacy US1 when only TwilioAuthToken is set; with neither, the recommended regional mode's settings.
        /// </summary>
        public IReadOnlyList<string> Missing { get; }

        /// <summary>
        /// True only when every required setting is present (regional API key or legacy auth token).
        /// The single rule for whether SMS may be offered or attempted; blank or whitespace in any required field means false.
        /// </summary>
        public bool CanSend => Mode == TwilioSmsMode.RegionalApiKey || Mode == TwilioSmsMode.LegacyUs1AuthToken;

        public static TwilioSmsConfig Resolve(string accountSid, string authToken, string fromNumber,
                                              string region, string edge, string apiKeySid, string apiKeySecret)
        {
            var missing = MissingSettings(accountSid, authToken, fromNumber, region, edge, apiKeySid, apiKeySecret);
            if (IsBlank(accountSid) || IsBlank(fromNumber))
                return NotConfigured(missing);

            var hasKey = !IsBlank(apiKeySid) || !IsBlank(apiKeySecret);
            if (!IsBlank(region))
            {
                if (IsBlank(apiKeySid) || IsBlank(apiKeySecret))
                    return Misconfigured("TwilioRegion is set, but TwilioApiKeySid or TwilioApiKeySecret is missing", missing);
                var r = region.Trim().ToLowerInvariant();
                var e = IsBlank(edge) ? DefaultEdge(r) : edge.Trim().ToLowerInvariant();
                if (e == null)
                    return Misconfigured("TwilioEdge is required for region " + r, missing);
                return new TwilioSmsConfig(TwilioSmsMode.RegionalApiKey, apiKeySid.Trim(), apiKeySecret, accountSid.Trim(),
                                           r, e, fromNumber.Trim(), null);
            }
            if (hasKey)
                return Misconfigured("TwilioApiKeySid/TwilioApiKeySecret are set without TwilioRegion", missing);
            if (IsBlank(authToken))
                return NotConfigured(missing);
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

        /// <summary>A configuration that could not be read at all; SMS is then not offered.</summary>
        public static TwilioSmsConfig Unreadable(string problem) => Misconfigured(problem);

        /// <summary>
        /// One line for the startup log: mode, region, edge and, when SMS cannot send, the names of the missing settings
        /// and the problem. Never a phone number, SID, key, secret or token.
        /// </summary>
        public string Describe()
        {
            var region = Region ?? (Mode == TwilioSmsMode.LegacyUs1AuthToken ? "us1" : "-");
            var line = $"SMS login: mode={Mode}, region={region}";
            if (Edge != null)
                line += $", edge={Edge}";
            if (!CanSend)
                line += $", missing={(Missing.Count == 0 ? "-" : string.Join(",", Missing))}";
            if (Problem != null)
                line += $", problem={Problem}";
            return line;
        }

        /// <summary>The blank required settings (names only) for the mode being set up; see <see cref="Missing"/>.</summary>
        private static IReadOnlyList<string> MissingSettings(string accountSid, string authToken, string fromNumber,
                                                             string region, string edge, string apiKeySid, string apiKeySecret)
        {
            var regionalAttempt = !IsBlank(region) || !IsBlank(edge) || !IsBlank(apiKeySid) || !IsBlank(apiKeySecret);
            var legacyAttempt = !regionalAttempt && !IsBlank(authToken);
            var missing = new List<string>();
            if (IsBlank(accountSid)) missing.Add("TwilioAccountSid");
            if (legacyAttempt)
            {
                // TwilioAuthToken is set (that is what makes it a legacy attempt)
                if (IsBlank(fromNumber)) missing.Add("TwilioFromNumber");
                return missing;
            }
            if (IsBlank(fromNumber)) missing.Add("TwilioFromNumber");
            if (IsBlank(region)) missing.Add("TwilioRegion");
            else if (IsBlank(edge) && DefaultEdge(region.Trim().ToLowerInvariant()) == null) missing.Add("TwilioEdge");
            if (IsBlank(apiKeySid)) missing.Add("TwilioApiKeySid");
            if (IsBlank(apiKeySecret)) missing.Add("TwilioApiKeySecret");
            return missing;
        }

        private static string DefaultEdge(string region) => region == "ie1" ? "dublin" : null;
        private static bool IsBlank(string s) => string.IsNullOrWhiteSpace(s);
        private static TwilioSmsConfig NotConfigured(IReadOnlyList<string> missing = null) => new TwilioSmsConfig(TwilioSmsMode.NotConfigured, null, null, null, null, null, null, null, missing);
        private static TwilioSmsConfig Misconfigured(string problem, IReadOnlyList<string> missing = null) => new TwilioSmsConfig(TwilioSmsMode.Misconfigured, null, null, null, null, null, null, problem, missing);
    }
}
