namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelraid
    [Serializable]
    public sealed class TwitchRequestEventSubChannelRaid
    {
        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = $"channel.raid";

        [JsonPropertyName(name: "version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName(name: "condition")]
        public TwitchConditionEventSubChannelRaid Condition { get; set; } = null;

        [JsonPropertyName(name: "transport")]
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