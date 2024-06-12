
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelRaid : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName("from_broadcaster_user_id")]
        public string from_broadcaster_user_id { get; set; } = string.Empty;

        [JsonPropertyName("from_broadcaster_user_login")]
        public string from_broadcaster_user_login { get; set; } = string.Empty;

        [JsonPropertyName("from_broadcaster_user_name")]
        public string from_broadcaster_user_name { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_id")]
        public string to_broadcaster_user_id { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_login")]
        public string to_broadcaster_user_login { get; set; } = string.Empty;

        [JsonPropertyName("to_broadcaster_user_name")]
        public string to_broadcaster_user_name { get; set; } = string.Empty;

        [JsonPropertyName("viewers")]
        public int? viewers { get; set; } = 0;
	}
}