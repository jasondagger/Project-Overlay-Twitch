
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationSubGift
	{
        [JsonPropertyName(name: "community_gift_id")]
        public string CommunityGiftId { get; set; } = string.Empty;

        [JsonPropertyName(name: "cumulative_total")]
        public int? CumulativeTotal { get; set; } = 0;

        [JsonPropertyName(name: "duration_months")]
        public int? DurationMonths { get; set; } = 0;

        [JsonPropertyName(name: "recipient_user_id")]
        public string RecipientUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "recipient_user_login")]
        public string RecipientUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "recipient_user_name")]
        public string RecipientUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "sub_tier")]
        public string SubTier { get; set; } = string.Empty;
	}
}