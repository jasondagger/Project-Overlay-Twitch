
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement
	{
        [JsonPropertyName(name: "color")]
        public string Color { get; set; } = string.Empty;
	}
}