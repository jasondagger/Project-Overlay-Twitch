
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
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