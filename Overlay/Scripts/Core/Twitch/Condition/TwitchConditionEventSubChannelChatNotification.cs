
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubChannelChatNotification
    {
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelChatNotification(
            string userId
        )
        {
            BroadcasterUserId = userId;
            UserId = userId;
        }
    }
}