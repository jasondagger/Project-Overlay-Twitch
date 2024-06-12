namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchEventSubTransportWebSocket
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = "websocket";

        [JsonPropertyName("session_id")]
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