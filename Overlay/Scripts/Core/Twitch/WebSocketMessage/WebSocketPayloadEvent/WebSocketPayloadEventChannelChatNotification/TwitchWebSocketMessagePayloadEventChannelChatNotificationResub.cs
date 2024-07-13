
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationResub
	{
        [JsonPropertyName("cumulative_months")]
        public int? CumulativeMonths = 0;

        [JsonPropertyName("duration_months")]
        public int? DurationMonths = 0;

        [JsonPropertyName("gifter_is_anonymous")]
        public bool? GifterIsAnonymous = false;

        [JsonPropertyName("gifter_user_id")]
        public string GifterUserId = string.Empty;

        [JsonPropertyName("gifter_user_login")]
        public string GifterUserLogin = string.Empty;

        [JsonPropertyName("gifter_user_name")]
        public string GifterUsername = string.Empty;

        [JsonPropertyName("is_gift")]
        public bool? IsGift = false;

        [JsonPropertyName("is_prime")]
        public bool? IsPrime = false;

        [JsonPropertyName("streak_months")]
        public int? StreakMonths = 0;

        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;
	}
}