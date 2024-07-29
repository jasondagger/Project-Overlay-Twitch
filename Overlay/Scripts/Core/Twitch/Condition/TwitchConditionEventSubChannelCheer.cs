
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubChannelCheer
    {
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelCheer(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}