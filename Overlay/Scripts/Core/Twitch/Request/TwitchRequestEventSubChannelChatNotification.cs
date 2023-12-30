
// https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchatnotification
[System.Serializable]
public sealed partial class TwitchRequestEventSubChannelChatNotification
{
    public string type { get; set; } = $"channel.chat.notification";
    public string version { get; set; } = $"1";
    public TwitchConditionEventSubChannelChatNotification condition { get; set; } = new();
    public TwitchEventSubTransportWebSocket transport { get; set; } = new();

    public TwitchRequestEventSubChannelChatNotification(
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