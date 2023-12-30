
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelsubscriptiongift
[System.Serializable]
public sealed class TwitchRequestEventSubChannelSubscriptionGift
{
    public string type = $"channel.subscription.gift";
    public string version = $"1";
    public TwitchConditionEventSubChannelSubscriptionGift condition = null;
    public TwitchEventSubTransportWebSocket transport = null;

    public TwitchRequestEventSubChannelSubscriptionGift(
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