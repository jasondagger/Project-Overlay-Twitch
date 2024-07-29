
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage
	{
        [JsonPropertyName(name: "fragments")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment[] Fragments { get; set; } = null;

        [JsonPropertyName(name: "text")]
        public string Text { get; set; } = string.Empty;
	}
}