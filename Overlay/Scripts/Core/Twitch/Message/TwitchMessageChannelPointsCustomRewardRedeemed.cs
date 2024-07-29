namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelPointsCustomRewardRedeemed : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed Event { get; set; } = null;

		public TwitchMessageChannelPointsCustomRewardRedeemed(
			TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed
		)
		{
			this.Event = @event;
		}
	}
}