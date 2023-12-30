
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelraid
[System.Serializable]
public sealed class TwitchRequestEventSubChannelRaid
{
    public string type = $"channel.raid";
    public string version = $"1";
    public TwitchConditionEventSubChannelRaid condition = null;
    public TwitchEventSubTransportWebSocket transport = null;

    public TwitchRequestEventSubChannelRaid(
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