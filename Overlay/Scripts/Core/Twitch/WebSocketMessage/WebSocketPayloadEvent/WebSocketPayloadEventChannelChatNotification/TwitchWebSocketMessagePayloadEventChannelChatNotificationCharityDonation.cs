
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonation
	{
        [JsonPropertyName("amount")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonationCharityAmount Amount = new();
       
        [JsonPropertyName("charity_name")]
        public string CharityName = string.Empty;
	}
}