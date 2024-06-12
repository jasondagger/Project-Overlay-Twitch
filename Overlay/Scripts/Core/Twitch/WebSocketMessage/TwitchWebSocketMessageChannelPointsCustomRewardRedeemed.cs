
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessageChannelPointsCustomRewardRedeemed : TwitchWebSocketMessage
	{
        [JsonPropertyName("payload")]
        public new TwitchWebSocketMessagePayloadChannelPointsCustomRewardRedeemed Payload { get; set; } = new();
	}
}