namespace Overlay
{
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelraid
    [System.Serializable]
    public sealed class TwitchRequestEventSubChannelRaid
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.raid";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelRaid Condition { get; set; } = null;

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelRaid(
            string userId,
            string sessionId
        )
        {
            Condition = new(
                userId: userId
            );
            Transport = new(
                sessionId: sessionId
            );
        }
    }
}