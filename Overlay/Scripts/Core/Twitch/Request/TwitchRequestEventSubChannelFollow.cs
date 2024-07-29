
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelfollow
    [Serializable]
    public sealed class TwitchRequestEventSubChannelFollow
    {
        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = $"channel.follow";

        [JsonPropertyName(name: "version")]
        public string Version { get; set; } = $"2";

        [JsonPropertyName(name: "condition")]
        public TwitchConditionEventSubChannelFollow Condition { get; set; } = null;

        [JsonPropertyName(name: "transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelFollow(
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