
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade
	{
        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;
	}
}