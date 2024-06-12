
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationGiftPaidUpgrade
	{
        [JsonPropertyName("gifter_is_anonymous")]
        public bool? GifterIsAnonymous = false;

        [JsonPropertyName("gifter_user_id")]
        public string GifterUserId = string.Empty;

        [JsonPropertyName("gifter_user_login")]
        public string GifterUserLogin = string.Empty;

        [JsonPropertyName("gifter_user_name")]
        public string GifterUsername = string.Empty;
	}
}