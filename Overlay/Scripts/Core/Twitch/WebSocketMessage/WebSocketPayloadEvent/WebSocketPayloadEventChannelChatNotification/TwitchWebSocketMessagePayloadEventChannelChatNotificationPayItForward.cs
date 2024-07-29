
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationPayItForward
	{
        [JsonPropertyName(name: "gifter_is_anonymous")]
        public bool? GifterIsAnonymous { get; set; } = false;

        [JsonPropertyName(name: "gifter_user_id")]
        public string GifterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_user_login")]
        public string GifterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_user_name")]
        public string GifterUsername { get; set; } = string.Empty;
	}
}