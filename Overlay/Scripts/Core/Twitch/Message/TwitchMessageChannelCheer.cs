namespace Overlay
{
    using System.Text.Json.Serialization;

    public sealed class TwitchMessageChannelCheer : TwitchMessage
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelCheer Event { get; set; } = new();

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