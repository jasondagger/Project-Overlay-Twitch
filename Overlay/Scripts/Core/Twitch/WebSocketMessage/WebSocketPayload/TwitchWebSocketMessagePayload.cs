
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public class TwitchWebSocketMessagePayload
    {
        [JsonPropertyName(name: "session")]
        public TwitchWebSocketMessagePayloadSession Session { get; set; } = new();

        [JsonPropertyName(name: "subscription")]
        public TwitchWebSocketMessagePayloadSubscription Subscription { get; set; } = new();
    }
}