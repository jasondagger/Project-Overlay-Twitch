
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public class TwitchWebSocketMessagePayload
    {
        [JsonPropertyName("session")]
        public TwitchWebSocketMessagePayloadSession Session { get; set; } = new();

        [JsonPropertyName("subscription")]
        public TwitchWebSocketMessagePayloadSubscription Subscription { get; set; } = new();
    }
}