
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubChannelFollow
    {
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "moderator_user_id")]
        public string ModeratorUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelFollow(
            string userId
        )
        {
            BroadcasterUserId = userId;
            ModeratorUserId = userId;
        }
    }
}