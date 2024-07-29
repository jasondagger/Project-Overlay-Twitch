
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationResub
	{
        [JsonPropertyName(name: "cumulative_months")]
        public int? CumulativeMonths { get; set; } = 0;

        [JsonPropertyName(name: "duration_months")]
        public int? DurationMonths { get; set; } = 0;

        [JsonPropertyName(name: "gifter_is_anonymous")]
        public bool? GifterIsAnonymous { get; set; } = false;

        [JsonPropertyName(name: "gifter_user_id")]
        public string GifterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_user_login")]
        public string GifterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_user_name")]
        public string GifterUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "is_gift")]
        public bool? IsGift { get; set; } = false;

        [JsonPropertyName(name: "is_prime")]
        public bool? IsPrime { get; set; } = false;

        [JsonPropertyName(name: "streak_months")]
        public int? StreakMonths { get; set; } = 0;

        [JsonPropertyName(name: "sub_tier")]
        public string SubTier { get; set; } = string.Empty;
	}
}