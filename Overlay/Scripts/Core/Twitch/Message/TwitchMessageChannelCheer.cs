
public sealed class TwitchMessageChannelCheer : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelCheer @event { get; set; } = new();

    public TwitchMessageChannelCheer(
        TwitchWebSocketMessagePayloadEventChannelCheer @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelCheer
    )
    {
        this.@event = @event;
    }
}