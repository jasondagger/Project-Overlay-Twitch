
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchannel_points_custom_reward_redemptionadd
    [Serializable]
    public sealed partial class TwitchRequestEventSubChannelPointsRedemption
    {
        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = $"channel.channel_points_custom_reward_redemption.add";

        [JsonPropertyName(name: "version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName(name: "condition")]
        public TwitchConditionEventSubChannelCheer Condition { get; set; } = null;

        [JsonPropertyName(name: "transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelPointsRedemption(
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