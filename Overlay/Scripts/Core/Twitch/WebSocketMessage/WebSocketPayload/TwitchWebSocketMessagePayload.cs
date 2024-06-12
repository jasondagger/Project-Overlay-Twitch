
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public class TwitchWebSocketMessagePayload
    {
        [JsonPropertyName("session")]
        public TwitchWebSocketMessagePayloadSession Session { get; set; } = new();

        [JsonPropertyName("subscription")]
        public TwitchWebSocketMessagePayloadSubscription Subscription { get; set; } = new();
    }
}