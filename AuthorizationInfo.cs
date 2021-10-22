using WebhookProcessor.Constants;

namespace WebhookProcessor
{
    public class AuthorizationInfo
    {
        public AuthorizationInfo(AuthorizationType authType)
        {
            AuthType = authType;
        }

        public AuthorizationType AuthType { get; set; }

       public string UserName { get; set; }

       public string Password { get; set; }

       public string AuthToken { get; set; }

       public string key { get; set; }

       public string Value { get; set; }

       public AttachApiKeyOptions Options { get; set; }

    }
}