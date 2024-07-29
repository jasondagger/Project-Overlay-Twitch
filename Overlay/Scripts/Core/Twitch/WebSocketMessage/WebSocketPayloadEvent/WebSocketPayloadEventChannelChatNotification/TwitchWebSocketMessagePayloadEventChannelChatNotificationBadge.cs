
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge
	{
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "info")]
        public string Info { get; set; } = string.Empty;

        [JsonPropertyName(name: "set_id")]
        public string SetId { get; set; } = string.Empty;
	}
}