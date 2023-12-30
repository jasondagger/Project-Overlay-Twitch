
public sealed class TwitchMessageChannelFollow : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelFollow @event { get; set; } = new();

    public TwitchMessageChannelFollow(
        TwitchWebSocketMessagePayloadEventChannelFollow @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelFollow
    )
    {
        this.@event = @event;
    }
}