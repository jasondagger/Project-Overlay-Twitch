
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public class TwitchWebSocketMessage
    {
        [JsonPropertyName(name: "metadata")]
        public TwitchWebSocketMessageMetadata Metadata { get; set; } = new();

        [JsonPropertyName(name: "payload")]
        public TwitchWebSocketMessagePayload Payload { get; set; } = new();
    }
}