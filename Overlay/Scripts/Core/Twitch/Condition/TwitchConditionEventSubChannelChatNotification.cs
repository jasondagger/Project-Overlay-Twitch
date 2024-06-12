
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubChannelChatNotification
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
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