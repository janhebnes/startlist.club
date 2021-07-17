using FlightJournal.Web.Extensions;
using FlightJournal.Web.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace FlightJournal.Web.FlightExport
{
    public interface IFlightExporterAuthenticator
    {
        bool Authenticate();
    }

    public interface ISuppliesToken
    {
        string Token { get; }
    }


    /// <summary>
    /// Obtains auth token by posting username and password as x-www-form-urlencoded data.
    /// Used by Foreningsadministration
    /// </summary>
    public class FormUrlEncodedTokenDeliveringFlightExporterAuthenticator : IFlightExporterAuthenticator, ISuppliesToken
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly RestClient _client;

        public FormUrlEncodedTokenDeliveringFlightExporterAuthenticator(string url, string username, string password)
        {
            _url = url;
            _client = new RestClient(_url);

            _username = username;
            _password = password;
        }

        public bool Authenticate()
        {
            if (_username.IsNullOrEmpty() || _password.IsNullOrEmpty())
                return false;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", _username);
            request.AddParameter("password", _password);
            var r = _client.Execute(request);
            if (r.ResponseStatus == ResponseStatus.Completed
                && (int)r.StatusCode >= 200
                && (int)r.StatusCode <= 299)
            {
                var t = JsonConvert.DeserializeAnonymousType(r.Content, new { access_token = "" });
                if (t.access_token.IsNullOrEmpty())
                {
                    Log.Warning($"{nameof(FormUrlEncodedTokenDeliveringFlightExporterAuthenticator)} got bad auth token from {_url} for user {_username}");
                    return false;
                }

                Token = t.access_token;
                return true;
            }

            return false;
        }

        public string Token { get; private set; }
    }


    public class DummyFlightExporterAuthenticator : IFlightExporterAuthenticator
    {
        public bool Authenticate()
        {
            return true;
        }
    }
}