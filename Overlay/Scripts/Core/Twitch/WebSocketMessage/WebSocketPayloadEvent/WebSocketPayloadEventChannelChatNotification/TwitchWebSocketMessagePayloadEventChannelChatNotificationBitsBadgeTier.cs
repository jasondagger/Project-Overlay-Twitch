
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier
	{
        [JsonPropertyName(name: "tier")]
        public int? Tier { get; set; } = 0;
	}
}