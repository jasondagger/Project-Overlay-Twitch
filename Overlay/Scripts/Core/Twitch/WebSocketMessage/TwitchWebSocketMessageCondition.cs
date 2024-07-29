
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchWebSocketMessageCondition
    {
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId = string.Empty;

        [JsonPropertyName(name: "moderator_user_id")]
        public string ModeratorUserId = string.Empty;

        [JsonPropertyName(name: "to_broadcaster_user_id")]
        public string ToBroadcasterUserId = string.Empty;
    }
}