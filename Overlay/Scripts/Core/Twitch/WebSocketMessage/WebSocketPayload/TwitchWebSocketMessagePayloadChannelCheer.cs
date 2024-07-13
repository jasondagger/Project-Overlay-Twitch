
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadChannelCheer : TwitchWebSocketMessagePayload
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelCheer Event { get; set; } = new();
	}
}