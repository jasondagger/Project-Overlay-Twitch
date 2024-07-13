
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelSubscriptionGift : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift Event { get; set; } = new();
	}
}