
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchannel_points_custom_reward_redemptionadd
[System.Serializable]
public sealed partial class TwitchRequestEventSubChannelPointsRedemption
{
    public string type = $"channel.channel_points_custom_reward_redemption.add";
    public string version = $"1";
    public TwitchConditionEventSubChannelCheer condition = new();
    public TwitchEventSubTransportWebSocket transport = new();

    public TwitchRequestEventSubChannelPointsRedemption(
        string userId,
        string sessionId
    )
    {
        condition = new(
            userId
        );
        transport = new(
            sessionId
        );
    }
}