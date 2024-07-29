namespace Overlay
{
	using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelRaid : TwitchMessage
	{
		[JsonPropertyName(name: "event")]
		public TwitchWebSocketMessagePayloadEventChannelRaid Event { get; set; } = null;

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