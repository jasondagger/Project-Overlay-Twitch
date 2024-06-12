
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCommunitySubGift
	{
        [JsonPropertyName("cumulative_total")]
        public int? CumulativeTotal = 0;

        [JsonPropertyName("id")]
        public string Id = string.Empty;

        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;

        [JsonPropertyName("total")]
        public int? Total = 0;
	}
}