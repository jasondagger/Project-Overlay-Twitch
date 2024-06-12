
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchWebSocketMessageCondition
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId = string.Empty;

        [JsonPropertyName("moderator_user_id")]
        public string ModeratorUserId = string.Empty;

        [JsonPropertyName("to_broadcaster_user_id")]
        public string ToBroadcasterUserId = string.Empty;
    }
}