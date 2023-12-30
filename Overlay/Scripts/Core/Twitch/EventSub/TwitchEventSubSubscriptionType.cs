
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#subscription-types
public enum TwitchEventSubSubscriptionType : int
{
    Unknown = -1,
    ChannelChatNotification,
    ChannelCheer,
    ChannelFollow,
    ChannelPointsCustomRewardRedeemed,
    ChannelRaid,
    ChannelSubscribe,
    ChannelSubscriptionGift,
}