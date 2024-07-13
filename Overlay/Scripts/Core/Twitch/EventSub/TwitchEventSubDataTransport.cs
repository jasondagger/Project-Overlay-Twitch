
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchEventSubDataTransport
    {
        public string method = string.Empty;
        public string callback = string.Empty;
        public string session_id = string.Empty;
        public string connected_at = string.Empty;
    }
}