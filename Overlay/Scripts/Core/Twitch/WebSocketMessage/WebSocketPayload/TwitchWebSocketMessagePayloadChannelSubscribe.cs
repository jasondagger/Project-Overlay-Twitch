
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelSubscribe : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscribe Event { get; set; } = new();
	}
}