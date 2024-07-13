
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement
	{
        [JsonPropertyName("color")]
        public string Color = string.Empty;
	}
}