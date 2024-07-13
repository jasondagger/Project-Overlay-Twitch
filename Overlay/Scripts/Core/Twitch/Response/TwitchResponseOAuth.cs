
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchResponseOAuth
    {
        public string access_token = string.Empty;
        public string expires_in = string.Empty;
        public string token_type = string.Empty;
    }
}