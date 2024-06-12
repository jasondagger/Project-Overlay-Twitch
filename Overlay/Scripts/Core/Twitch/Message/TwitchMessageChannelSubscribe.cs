namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelSubscribe : TwitchMessage
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscribe Event { get; set; } = new();

		public TwitchMessageChannelSubscribe(
			TwitchWebSocketMessagePayloadEventChannelSubscribe @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelSubscribe
		)
		{
			this.Event = @event;
		}
	}
}