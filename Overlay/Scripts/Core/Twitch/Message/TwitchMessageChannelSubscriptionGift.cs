namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelSubscriptionGift : TwitchMessage
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift Event { get; set; } = new();

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