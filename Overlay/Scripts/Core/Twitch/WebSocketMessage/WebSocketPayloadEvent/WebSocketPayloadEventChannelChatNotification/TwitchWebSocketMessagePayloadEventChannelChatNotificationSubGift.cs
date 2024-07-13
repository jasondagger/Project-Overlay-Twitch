
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationSubGift
	{
        [JsonPropertyName("community_gift_id")]
        public string CommunityGiftId = string.Empty;

        [JsonPropertyName("cumulative_total")]
        public int? CumulativeTotal = 0;

        [JsonPropertyName("duration_months")]
        public int? DurationMonths = 0;

        [JsonPropertyName("recipient_user_id")]
        public string RecipientUserId = string.Empty;

        [JsonPropertyName("recipient_user_login")]
        public string RecipientUserLogin = string.Empty;

        [JsonPropertyName("recipient_user_name")]
        public string RecipientUsername = string.Empty;

        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;
	}
}