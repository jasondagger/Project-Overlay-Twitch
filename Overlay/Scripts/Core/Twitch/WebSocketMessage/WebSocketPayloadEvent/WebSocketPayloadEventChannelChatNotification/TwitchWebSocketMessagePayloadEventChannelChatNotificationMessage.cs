
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage
	{
        [JsonPropertyName("fragments")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment[] Fragments = null;

        [JsonPropertyName("text")]
        public string Text = string.Empty;
	}
}