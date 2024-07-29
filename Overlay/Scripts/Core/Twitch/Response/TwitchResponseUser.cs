
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUser
    {
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName(name: "display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_type")]
        public string BroadcasterType { get; set; } = string.Empty;

        [JsonPropertyName(name: "description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName(name: "profile_image_url")]
        public string ProfileImageUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "offline_image_url")]
        public string OfflineImageUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "view_count")]
        public int ViewCount { get; set; } = 0;

        [JsonPropertyName(name: "email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName(name: "created_at")]
        public string CreatedAt { get; set; } = string.Empty;
    }
}