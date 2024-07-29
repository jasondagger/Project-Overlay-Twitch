namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelCheer : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
        public TwitchWebSocketMessagePayloadEventChannelCheer Event { get; set; } = null;

		public TwitchMessageChannelCheer(
			TwitchWebSocketMessagePayloadEventChannelCheer @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelCheer
		)
		{
			this.Event = @event;
		}
	}
}