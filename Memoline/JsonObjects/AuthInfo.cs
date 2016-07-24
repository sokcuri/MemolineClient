namespace Memoline.JsonObjects
{
    internal class AuthInfo
    {
        public string oauth_token { get; set; }
        public string oauth_token_secret { get; set; }
        public string authorize_url { get; set; }
    }

    internal class AuthCallBack
    {
        public string user_token { get; set; }
        public string user_secret { get; set; }
        public string user_type { get; set; }
    }
}