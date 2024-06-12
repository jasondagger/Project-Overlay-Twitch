namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelPointsCustomRewardRedeemed : TwitchMessage
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed Event { get; set; } = new();

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