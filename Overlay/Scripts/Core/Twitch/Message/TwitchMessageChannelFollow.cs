namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelFollow : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelFollow Event { get; set; } = null;

		public TwitchMessageChannelFollow(
			TwitchWebSocketMessagePayloadEventChannelFollow @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelFollow
		)
		{
			this.Event = @event;
		}
	}
}