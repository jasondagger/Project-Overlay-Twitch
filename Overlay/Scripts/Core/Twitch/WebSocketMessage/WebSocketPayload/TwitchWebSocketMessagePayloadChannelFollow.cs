
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelFollow : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelFollow Event { get; set; } = new();
	}
}