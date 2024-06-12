
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubChannelSubscriptionGift
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelSubscriptionGift(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}