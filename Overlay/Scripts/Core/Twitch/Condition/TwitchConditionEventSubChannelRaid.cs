namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubChannelRaid
    {
        [JsonPropertyName("to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelRaid(
            string userId
        )
        {
            ToBroadcasterUserId = userId;
        }
    }
}