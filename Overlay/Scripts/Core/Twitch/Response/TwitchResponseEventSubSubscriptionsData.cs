
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseEventSubSubscriptionsData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubSubscriptions Condition { get; set; } = null;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        [JsonPropertyName("cost")]
        public int Cost { get; set; } = 0;
    }
}