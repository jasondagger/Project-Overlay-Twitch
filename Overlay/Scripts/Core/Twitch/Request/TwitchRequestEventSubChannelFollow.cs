
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelfollow
[System.Serializable]
public sealed class TwitchRequestEventSubChannelFollow
{
    public string type = $"channel.follow";
    public string version = $"2";
    public TwitchConditionEventSubChannelFollow condition = null;
    public TwitchEventSubTransportWebSocket transport = null;

    public TwitchRequestEventSubChannelFollow(
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