
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelfollow
    [Serializable]
    public sealed class TwitchRequestEventSubChannelFollow
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.follow";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"2";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelFollow Condition { get; set; } = null;

        [JsonPropertyName("transport")]
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