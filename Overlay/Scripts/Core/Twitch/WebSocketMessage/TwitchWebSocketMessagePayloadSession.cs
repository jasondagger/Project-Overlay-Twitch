
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchWebSocketMessagePayloadSession
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("keepalive_timeout_seconds")]
        public int KeepaliveTimeoutSeconds { get; set; } = 0;

        [JsonPropertyName("reconnect_url")]
        public string ReconnectUrl { get; set; } = string.Empty;

        [JsonPropertyName("connected_at")]
        public string ConnectedAt { get; set; } = string.Empty;
    }
}