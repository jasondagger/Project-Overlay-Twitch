
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationRaid
	{
        [JsonPropertyName("profile_image_url")]
        public string ProfileImageUrl = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId = string.Empty;

        [JsonPropertyName("user_login")]
        public string UserLogin = string.Empty;

        [JsonPropertyName("user_name")]
        public string UserName = string.Empty;

        [JsonPropertyName("viewer_count")]
        public int? ViewerCount = 0;
	}
}