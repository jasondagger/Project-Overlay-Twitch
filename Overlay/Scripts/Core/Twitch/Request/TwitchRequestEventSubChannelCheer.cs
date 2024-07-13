
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelcheer
    [Serializable]
    public sealed partial class TwitchRequestEventSubChannelCheer
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.cheer";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelCheer Condition { get; set; } = null;

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelCheer(
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