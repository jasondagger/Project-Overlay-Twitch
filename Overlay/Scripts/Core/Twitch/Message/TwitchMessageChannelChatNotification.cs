namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchMessageChannelChatNotification : TwitchMessage
	{
        [JsonPropertyName(name: "event")]
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