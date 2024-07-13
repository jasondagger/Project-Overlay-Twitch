
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchEventSubData
    {
        public string id = string.Empty;
        public string status = string.Empty;
        public string type = string.Empty;
        public string version = string.Empty;
        public string condition = string.Empty;
        public string created_at = string.Empty;
        public TwitchEventSubDataTransport transport = null;
        public int cost = 0;
    }
}