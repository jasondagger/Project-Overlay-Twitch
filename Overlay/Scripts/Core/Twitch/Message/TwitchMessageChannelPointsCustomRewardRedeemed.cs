
public sealed class TwitchMessageChannelPointsCustomRewardRedeemed : TwitchMessage
{
    public TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event { get; set; } = new();

    public TwitchMessageChannelPointsCustomRewardRedeemed(
        TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event
    ) : base(
        TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed
    )
    {
        this.@event = @event;
    }
}