
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelPointsCustomRewardRedeemed : TwitchWebSocketMessage
	{
        [JsonPropertyName(name: "payload")]
        public new TwitchWebSocketMessagePayloadChannelPointsCustomRewardRedeemed Payload { get; set; } = new();
	}
}