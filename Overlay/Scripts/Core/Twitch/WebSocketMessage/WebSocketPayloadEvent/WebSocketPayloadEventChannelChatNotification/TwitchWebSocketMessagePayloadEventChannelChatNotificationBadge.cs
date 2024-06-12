
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge
	{
        [JsonPropertyName("id")]
        public string Id = string.Empty;

        [JsonPropertyName("info")]
        public string Info = string.Empty;

        [JsonPropertyName("set_id")]
        public string SetId = string.Empty;
	}
}