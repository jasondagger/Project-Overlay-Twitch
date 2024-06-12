namespace Overlay
{
	using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelRaid : TwitchMessage
	{
		[JsonPropertyName("event")]
		public TwitchWebSocketMessagePayloadEventChannelRaid Event { get; set; } = new();

		public TwitchMessageChannelRaid(
			TwitchWebSocketMessagePayloadEventChannelRaid @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelRaid
		)
		{
			this.Event = @event;
		}
	}
}