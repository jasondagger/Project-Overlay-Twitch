
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchWebSocketMessagePayloadSubscription
    {
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName(name: "cost")]
        public int Cost { get; set; } = 0;

        [JsonPropertyName(name: "condition")]
        public TwitchWebSocketMessageCondition Condition { get; set; } = new();

        [JsonPropertyName(name: "transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = new();

        [JsonPropertyName(name: "created_at")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}