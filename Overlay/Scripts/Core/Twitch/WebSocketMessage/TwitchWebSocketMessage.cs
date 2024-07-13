
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public class TwitchWebSocketMessage
    {
        [JsonPropertyName("metadata")]
        public TwitchWebSocketMessageMetadata Metadata { get; set; } = new();

        [JsonPropertyName("payload")]
        public TwitchWebSocketMessagePayload Payload { get; set; } = new();
    }
}