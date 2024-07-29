
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelPointsCustomRewardRedeemed : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed Event { get; set; } = null;
	}
}