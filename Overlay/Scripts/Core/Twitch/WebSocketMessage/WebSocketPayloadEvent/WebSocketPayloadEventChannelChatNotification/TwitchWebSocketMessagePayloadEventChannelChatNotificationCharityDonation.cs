
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonation
	{
        [JsonPropertyName(name: "amount")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonationCharityAmount Amount { get; set; } = null;
       
        [JsonPropertyName(name: "charity_name")]
        public string CharityName { get; set; } = string.Empty;
	}
}