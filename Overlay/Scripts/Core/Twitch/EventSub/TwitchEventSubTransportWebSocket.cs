namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchEventSubTransportWebSocket
    {
        [JsonPropertyName(name: "method")]
        public string Method { get; set; } = "websocket";

        [JsonPropertyName(name: "session_id")]
        public string SessionId { get; set; } = string.Empty;

        public TwitchEventSubTransportWebSocket()
        {

        }

        public TwitchEventSubTransportWebSocket(
            string sessionId
        )
        {
            this.SessionId = sessionId;
        }
    }
}