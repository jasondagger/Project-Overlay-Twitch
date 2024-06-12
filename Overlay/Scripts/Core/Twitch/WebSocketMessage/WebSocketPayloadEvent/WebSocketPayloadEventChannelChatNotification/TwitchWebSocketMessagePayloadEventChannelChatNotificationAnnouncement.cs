
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement
	{
        [JsonPropertyName("color")]
        public string Color = string.Empty;
	}
}