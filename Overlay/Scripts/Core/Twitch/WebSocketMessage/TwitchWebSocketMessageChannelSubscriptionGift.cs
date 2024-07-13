
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelSubscriptionGift : TwitchWebSocketMessage
	{
        [JsonPropertyName("payload")]
        public new TwitchWebSocketMessagePayloadChannelSubscriptionGift Payload { get; set; } = new();
	}
}