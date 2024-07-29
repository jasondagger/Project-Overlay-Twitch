
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchWebSocketMessagePayloadSession
    {
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName(name: "keepalive_timeout_seconds")]
        public int KeepaliveTimeoutSeconds { get; set; } = 0;

        [JsonPropertyName(name: "reconnect_url")]
        public string ReconnectUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "connected_at")]
        public string ConnectedAt { get; set; } = string.Empty;
    }
}