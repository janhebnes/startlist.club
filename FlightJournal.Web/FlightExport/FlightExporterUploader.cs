using RestSharp;

namespace FlightJournal.Web.FlightExport
{
    /// <summary>
    /// Uploads data to impl defined destination using impl defined method.
    /// </summary>
    public interface IFlightExporterUploader
    {
        bool Upload(string formattedData);

        /// <summary>
        /// Use authenticator to get any required authentication info to use when uploading
        /// </summary>
        /// <param name="authenticator"></param>
        void SetAuthInfoFrom(IFlightExporterAuthenticator authenticator);
    }



    /// <summary>
    /// Basic uploader, posts string data to destination. No authentication.
    /// </summary>
    public class BaseUploader : IFlightExporterUploader
    {
        private readonly string _contentType;
        private readonly RestClient _client;

        public BaseUploader(string url, string contentType)
        {
            _contentType = contentType;
            _client = new RestClient(url);
        }

        public bool Upload(string formattedData)
        {
            var request = new RestRequest(Method.POST);
            request.AddParameter(_contentType, formattedData, ParameterType.RequestBody);
            var r = _client.Execute(request);
            return (r.ResponseStatus == ResponseStatus.Completed
                    && (int)r.StatusCode >= 200
                    && (int)r.StatusCode <= 299);
        }

        public virtual void SetAuthInfoFrom(IFlightExporterAuthenticator authenticator)
        {
            // no action
        }

        protected virtual void BuildHeaders(RestRequest request)
        {
            request.AddHeader("Content-Type", _contentType);
        }
    }

    /// <summary>
    /// Uploader authenticating via "Authorization: Bearer" header.
    /// Used by Foreningsadministration
    /// </summary>
    public class BearerAuthUploader : BaseUploader
    {
        private string _token;
        public BearerAuthUploader(string url, string contentType) : base(url, contentType)
        {
        }

        protected override void BuildHeaders(RestRequest request)
        {
            base.BuildHeaders(request);
            request.AddHeader("Authorization", $"Bearer {_token}");
        }
        public override void SetAuthInfoFrom(IFlightExporterAuthenticator authenticator)
        {
            if (authenticator is ISuppliesToken ts)
            {
                _token = ts.Token;
            }
        }
    }
}