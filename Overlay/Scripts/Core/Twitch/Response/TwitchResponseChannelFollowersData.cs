
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseChannelFollowersData
    {
        [JsonPropertyName(name: "followed_at")]
        public string FollowedAt { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string Username { get; set; } = string.Empty;
    }
}