
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchWebSocketMessageMetadata
    {
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("message_type")]
        public string MessageType { get; set; } = string.Empty;

        [JsonPropertyName("message_timestamp")]
        public string MessageTimestamp { get; set; } = string.Empty;

        [JsonPropertyName("subscription_type")]
        public string SubscriptionType { get; set; } = string.Empty;

        [JsonPropertyName("subscription_version")]
        public string SubscriptionVersion { get; set; } = string.Empty;
    }
}