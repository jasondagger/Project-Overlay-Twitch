
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubSubscriptions
    {
        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId = string.Empty;

        public TwitchConditionEventSubSubscriptions(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}