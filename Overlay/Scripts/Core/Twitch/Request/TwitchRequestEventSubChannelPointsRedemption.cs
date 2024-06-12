
namespace Overlay
{
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchannel_points_custom_reward_redemptionadd
    [System.Serializable]
    public sealed partial class TwitchRequestEventSubChannelPointsRedemption
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.channel_points_custom_reward_redemption.add";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelCheer Condition { get; set; } = null;

        [JsonPropertyName("transport")]
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