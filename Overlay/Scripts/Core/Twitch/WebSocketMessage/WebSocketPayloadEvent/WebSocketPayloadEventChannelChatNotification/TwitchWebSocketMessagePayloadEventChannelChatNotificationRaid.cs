
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationRaid
	{
        [JsonPropertyName(name: "profile_image_url")]
        public string ProfileImageUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "viewer_count")]
        public int? ViewerCount { get; set; } = 0;
	}
}