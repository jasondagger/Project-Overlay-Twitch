
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelCheer : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName("bits")]
        public int? Bits { get; set; } = 0;

        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName("is_anonymous")]
        public bool IsAnonymous { get; set; } = false;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string Username { get; set; } = string.Empty;
	}
}