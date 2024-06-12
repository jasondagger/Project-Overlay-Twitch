
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessageChannelSubscribe : TwitchWebSocketMessage
	{
        [JsonPropertyName("payload")]
        public new TwitchWebSocketMessagePayloadChannelSubscribe Payload { get; set; } = new();
	}
}