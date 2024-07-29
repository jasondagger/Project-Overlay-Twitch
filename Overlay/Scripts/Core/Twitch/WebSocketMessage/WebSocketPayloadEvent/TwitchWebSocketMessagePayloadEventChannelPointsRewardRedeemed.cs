
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "redeemed_at")]
        public string RedeemedAt { get; set; } = string.Empty;

        [JsonPropertyName(name: "reward")]
        public TwitchEventSubReward Reward { get; set; } = null;

        [JsonPropertyName(name: "status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_input")]
        public string UserInput { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string UserName { get; set; } = string.Empty;
	}
}