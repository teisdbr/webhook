using System.ComponentModel;

namespace WebhookProcessor.Constants
{
    public enum AuthorizationType
    {
        [Description("NoAuth")]
        NoAuth,
        [Description("Bearer")]
        Bearer,
        [Description("Basic")]
        Basic,
        [Description("XAPIKey")]
        XAPIKey
    }
}