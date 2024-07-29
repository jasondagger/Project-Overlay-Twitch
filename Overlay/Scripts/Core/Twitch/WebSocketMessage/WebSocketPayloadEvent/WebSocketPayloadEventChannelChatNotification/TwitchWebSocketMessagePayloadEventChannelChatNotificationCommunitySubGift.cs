
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCommunitySubGift
	{
        [JsonPropertyName(name: "cumulative_total")]
        public int? CumulativeTotal { get; set; } = 0;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "sub_tier")]
        public string SubTier { get; set; } = string.Empty;

        [JsonPropertyName(name: "total")]
        public int? Total { get; set; } = 0;
	}
}