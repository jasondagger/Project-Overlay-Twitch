
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchWebSocketMessagePayloadSubscription
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("cost")]
        public int Cost { get; set; } = 0;

        [JsonPropertyName("condition")]
        public TwitchWebSocketMessageCondition Condition { get; set; } = new();

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = new();

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}