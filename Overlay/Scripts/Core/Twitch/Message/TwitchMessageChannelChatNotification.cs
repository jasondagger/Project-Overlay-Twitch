
[System.Serializable]
public sealed class TwitchMessageChannelChatNotification : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelChatNotification @event { get; set; } = new();

    public TwitchMessageChannelChatNotification(
        TwitchWebSocketMessagePayloadEventChannelChatNotification @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelChatNotification
    )
    {
        this.@event = @event;
    }
}