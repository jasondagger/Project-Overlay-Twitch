
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchEventSubTransportWebHook
    {
        public string method = "webhook";
        public string callback = string.Empty;
        public string secret = string.Empty;

        public TwitchEventSubTransportWebHook(
            string callback,
            string secret
        )
        {
            this.callback = callback;
            this.secret = secret;
        }
    }
}