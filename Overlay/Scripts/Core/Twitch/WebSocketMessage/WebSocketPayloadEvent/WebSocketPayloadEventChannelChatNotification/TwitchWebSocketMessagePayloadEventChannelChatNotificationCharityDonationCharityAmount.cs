
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonationCharityAmount
	{
        [JsonPropertyName("currency")]
        public string Currency = string.Empty;

        [JsonPropertyName("decimal_places")]
        public int? DecimalPlaces = 0;

        [JsonPropertyName("value")]
        public int? Value = 0;
	}
}