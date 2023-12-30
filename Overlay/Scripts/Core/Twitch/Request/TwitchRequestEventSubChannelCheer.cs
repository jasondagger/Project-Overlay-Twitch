
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelcheer
[System.Serializable]
public sealed partial class TwitchRequestEventSubChannelCheer
{
    public string type { get; set; } = $"channel.cheer";
    public string version { get; set; } = $"1";
    public TwitchConditionEventSubChannelCheer condition { get; set; } = new();
    public TwitchEventSubTransportWebSocket transport { get; set; } = new();

    public TwitchRequestEventSubChannelCheer(
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