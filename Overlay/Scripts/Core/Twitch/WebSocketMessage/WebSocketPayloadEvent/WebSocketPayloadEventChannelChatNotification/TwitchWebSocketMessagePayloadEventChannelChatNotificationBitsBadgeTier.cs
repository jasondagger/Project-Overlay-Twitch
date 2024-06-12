
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier
	{
        [JsonPropertyName("tier")]
        public int? Tier = 0;
	}
}