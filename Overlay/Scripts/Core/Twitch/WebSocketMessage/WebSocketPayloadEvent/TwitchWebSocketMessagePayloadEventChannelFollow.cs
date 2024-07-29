
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelFollow : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "followed_at")]
        public string FollowedAt { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string UserName { get; set; } = string.Empty;
	}
}