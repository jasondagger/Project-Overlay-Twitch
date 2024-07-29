namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelSubscriptionGift : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift Event { get; set; } = null;

		public TwitchMessageChannelSubscriptionGift(
			TwitchWebSocketMessagePayloadEventChannelSubscriptionGift @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelSubscriptionGift
		)
		{
			this.Event = @event;
		}
	}
}