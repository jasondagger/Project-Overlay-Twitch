
public sealed class TwitchMessageChannelSubscribe : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelSubscribe @event { get; set; } = new();

    public TwitchMessageChannelSubscribe(
        TwitchWebSocketMessagePayloadEventChannelSubscribe @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelSubscribe
    )
    {
        this.@event = @event;
    }
}