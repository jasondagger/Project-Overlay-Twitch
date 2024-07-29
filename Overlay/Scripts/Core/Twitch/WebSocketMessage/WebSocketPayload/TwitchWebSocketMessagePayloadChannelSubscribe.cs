
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelSubscribe : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscribe Event { get; set; } = new();
	}
}