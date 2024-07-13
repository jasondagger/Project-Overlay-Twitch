
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelRaid : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName("from_broadcaster_user_id")]
        public string FromBroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName("from_broadcaster_user_login")]
        public string FromBroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName("from_broadcaster_user_name")]
        public string FromBroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_login")]
        public string ToBroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_name")]
        public string ToBroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName("viewers")]
        public int? Viewers { get; set; } = 0;
	}
}