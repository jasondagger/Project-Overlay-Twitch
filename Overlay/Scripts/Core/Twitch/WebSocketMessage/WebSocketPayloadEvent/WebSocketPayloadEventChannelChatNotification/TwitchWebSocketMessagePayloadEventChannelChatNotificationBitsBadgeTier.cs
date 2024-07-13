
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier
	{
        [JsonPropertyName("tier")]
        public int? Tier = 0;
	}
}