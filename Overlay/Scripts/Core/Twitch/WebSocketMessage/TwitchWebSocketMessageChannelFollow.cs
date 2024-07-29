
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelFollow : TwitchWebSocketMessage
	{
        [JsonPropertyName(name: "payload")]
        public new TwitchWebSocketMessagePayloadChannelFollow Payload { get; set; } = new();
	}
}