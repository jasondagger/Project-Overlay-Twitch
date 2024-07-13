namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelchatnotification
    [Serializable]
    public sealed class TwitchRequestEventSubChannelChatNotification
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = $"channel.chat.notification";

        [JsonPropertyName("version")]
        public string Version { get; set; } = $"1";

        [JsonPropertyName("condition")]
        public TwitchConditionEventSubChannelChatNotification Condition { get; set; } = null;

        [JsonPropertyName("transport")]
        public TwitchEventSubTransportWebSocket Transport { get; set; } = null;

        public TwitchRequestEventSubChannelChatNotification(
            string userId,
            string sessionId
        )
        {
            Condition = new(
                userId: userId
            );
            Transport = new(
                sessionId: sessionId
            );
        }
    }
}