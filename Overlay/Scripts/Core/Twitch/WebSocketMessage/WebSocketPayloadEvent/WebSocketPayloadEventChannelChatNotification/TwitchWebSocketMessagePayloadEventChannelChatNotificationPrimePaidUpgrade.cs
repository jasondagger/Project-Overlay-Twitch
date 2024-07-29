
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade
	{
        [JsonPropertyName(name: "sub_tier")]
        public string SubTier { get; set; } = string.Empty;
	}
}