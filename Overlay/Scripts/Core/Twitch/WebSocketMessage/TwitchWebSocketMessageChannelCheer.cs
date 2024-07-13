
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelCheer : TwitchWebSocketMessage
	{
        [JsonPropertyName("payload")]
        public new TwitchWebSocketMessagePayloadChannelCheer Payload { get; set; } = new();
	}
}