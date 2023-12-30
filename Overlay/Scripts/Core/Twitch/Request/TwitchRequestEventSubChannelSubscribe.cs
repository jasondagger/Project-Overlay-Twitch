
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelsubscribe
[System.Serializable]
public sealed class TwitchRequestEventSubChannelSubscribe
{
    public string type = $"channel.subscribe";
    public string version = $"1";
    public TwitchConditionEventSubChannelSubscribe condition = null;
    public TwitchEventSubTransportWebSocket transport = null;

    public TwitchRequestEventSubChannelSubscribe(
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