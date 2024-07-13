
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseChannelFollowersData
    {
        [JsonPropertyName("followed_at")]
        public string FollowedAt { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string Username { get; set; } = string.Empty;
    }
}