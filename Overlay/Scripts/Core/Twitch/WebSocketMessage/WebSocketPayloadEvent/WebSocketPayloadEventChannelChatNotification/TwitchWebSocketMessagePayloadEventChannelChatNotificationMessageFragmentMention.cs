
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentMention
	{
        [JsonPropertyName("user_id")]
        public string UserId = string.Empty;

        [JsonPropertyName("user_login")]
        public string UserLogin = string.Empty;

        [JsonPropertyName("user_name")]
        public string Username = string.Empty;
	}
}