namespace Overlay
{
	using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchMessageChannelChatNotification : TwitchMessage
	{
        [JsonPropertyName("event")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotification Event { get; set; } = null;

		public TwitchMessageChannelChatNotification(
			TwitchWebSocketMessagePayloadEventChannelChatNotification @event
		) : base(
			TwitchEventSubSubscriptionType.ChannelChatNotification
		)
		{
			this.Event = @event;
		}
	}
}