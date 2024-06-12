
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationSub
	{
        [JsonPropertyName("duration_months")]
        public int? DurationMonths = 0;

        [JsonPropertyName("is_prime")]
        public bool? IsPrime = false;

        [JsonPropertyName("sub_tier")]
        public string SubTier = string.Empty;
	}
}