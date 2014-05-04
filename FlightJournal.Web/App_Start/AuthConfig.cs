using Microsoft.Web.WebPages.OAuth;

namespace FlightJournal.Web
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: "dKwmoLMH1zkqnvMblCMcQ",
                consumerSecret: "G71eTw0Cm1s0ygVQygPQrw7ckSCR4WfbBWxGWqfiO4");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "266255723422800",
                appSecret: "d564ffde30d649ca216f604cae5cbea3");

            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
