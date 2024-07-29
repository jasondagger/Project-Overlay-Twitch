
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationSub
	{
        [JsonPropertyName(name: "duration_months")]
        public int? DurationMonths { get; set; } = 0;

        [JsonPropertyName(name: "is_prime")]
        public bool? IsPrime { get; set; } = false;

        [JsonPropertyName(name: "sub_tier")]
        public string SubTier { get; set; } = string.Empty;
	}
}