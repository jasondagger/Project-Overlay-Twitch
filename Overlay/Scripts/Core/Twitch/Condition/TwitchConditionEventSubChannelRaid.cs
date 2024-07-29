namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchConditionEventSubChannelRaid
    {
        [JsonPropertyName(name: "to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelRaid(
            string userId
        )
        {
            ToBroadcasterUserId = userId;
        }
    }
}