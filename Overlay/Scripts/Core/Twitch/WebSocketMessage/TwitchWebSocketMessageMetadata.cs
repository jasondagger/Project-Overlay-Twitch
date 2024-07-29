
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchWebSocketMessageMetadata
    {
        [JsonPropertyName(name: "message_id")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName(name: "message_type")]
        public string MessageType { get; set; } = string.Empty;

        [JsonPropertyName(name: "message_timestamp")]
        public string MessageTimestamp { get; set; } = string.Empty;

        [JsonPropertyName(name: "subscription_type")]
        public string SubscriptionType { get; set; } = string.Empty;

        [JsonPropertyName(name: "subscription_version")]
        public string SubscriptionVersion { get; set; } = string.Empty;
    }
}