
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelSubscribe : TwitchWebSocketMessage
	{
        [JsonPropertyName(name: "payload")]
        public new TwitchWebSocketMessagePayloadChannelSubscribe Payload { get; set; } = new();
	}
}