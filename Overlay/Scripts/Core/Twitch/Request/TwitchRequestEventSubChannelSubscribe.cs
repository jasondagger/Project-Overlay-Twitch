
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelsubscribe
    [Serializable]
    public sealed class TwitchRequestEventSubChannelSubscribe
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.subscribe";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelSubscribe Condition { get; set; } = null;

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelSubscribe(
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