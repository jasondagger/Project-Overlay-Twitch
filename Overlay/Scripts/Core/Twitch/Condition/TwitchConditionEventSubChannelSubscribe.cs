
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubChannelSubscribe
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelSubscribe(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}