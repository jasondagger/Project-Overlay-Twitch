
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelChatNotification : TwitchWebSocketMessage
	{
        [JsonPropertyName(name: "payload")]
        public new TwitchWebSocketMessagePayloadChannelChatNotification Payload { get; set; } = new();
	}
}