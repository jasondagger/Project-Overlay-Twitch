namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelSubscribe : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelSubscribe Event { get; set; } = null;

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