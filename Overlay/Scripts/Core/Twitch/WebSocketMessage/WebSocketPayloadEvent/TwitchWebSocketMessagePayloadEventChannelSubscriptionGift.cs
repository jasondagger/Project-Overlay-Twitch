
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelSubscriptionGift : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "cumulative_total")]
        public int? CumulativeTotal { get; set; } = 0;

        [JsonPropertyName(name: "is_anonymous")]
        public bool? IsAnonymous { get; set; } = false;

        [JsonPropertyName(name: "tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName(name: "total")]
        public int? Total { get; set; } = 0;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string UserName { get; set; } = string.Empty;
	}
}