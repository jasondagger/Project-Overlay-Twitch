
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessageChannelRaid : TwitchWebSocketMessage
	{
        [JsonPropertyName("payload")]
        public new TwitchWebSocketMessagePayloadChannelRaid Payload { get; set; } = new();
	}
}