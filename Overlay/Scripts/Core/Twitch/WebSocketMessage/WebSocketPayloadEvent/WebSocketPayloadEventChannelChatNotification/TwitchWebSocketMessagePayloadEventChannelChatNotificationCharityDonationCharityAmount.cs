
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonationCharityAmount
	{
        [JsonPropertyName(name: "currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName(name: "decimal_places")]
        public int? DecimalPlaces { get; set; } = 0;

        [JsonPropertyName(name: "value")]
        public int? Value { get; set; } = 0;
	}
}