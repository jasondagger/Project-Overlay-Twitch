
public sealed class TwitchMessageChannelRaid : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelRaid @event { get; set; } = new();

    public TwitchMessageChannelRaid(
        TwitchWebSocketMessagePayloadEventChannelRaid @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelRaid
    )
    {
        this.@event = @event;
    }
}