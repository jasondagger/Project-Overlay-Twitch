
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelRaid : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName(name: "from_broadcaster_user_id")]
        public string FromBroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "from_broadcaster_user_login")]
        public string FromBroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "from_broadcaster_user_name")]
        public string FromBroadcasterUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "to_broadcaster_user_login")]
        public string ToBroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "to_broadcaster_user_name")]
        public string ToBroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "viewers")]
        public int? Viewers { get; set; } = 0;
	}
}