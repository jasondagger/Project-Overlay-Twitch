
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade
	{
        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;
	}
}