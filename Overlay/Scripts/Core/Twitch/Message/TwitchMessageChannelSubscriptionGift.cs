
public sealed class TwitchMessageChannelSubscriptionGift : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift @event { get; set; } = new();

    public TwitchMessageChannelSubscriptionGift(
        TwitchWebSocketMessagePayloadEventChannelSubscriptionGift @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelSubscriptionGift
    )
    {
        this.@event = @event;
    }
}