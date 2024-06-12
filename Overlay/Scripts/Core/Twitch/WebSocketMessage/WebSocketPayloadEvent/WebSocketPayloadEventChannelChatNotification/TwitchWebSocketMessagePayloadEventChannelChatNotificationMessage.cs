
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage
	{
        [JsonPropertyName("fragments")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment[] Fragments = null;

        [JsonPropertyName("text")]
        public string Text = string.Empty;
	}
}