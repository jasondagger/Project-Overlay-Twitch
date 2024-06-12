
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelSubscriptionGift : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift Event { get; set; } = new();
	}
}