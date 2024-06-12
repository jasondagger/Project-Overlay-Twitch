
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
	public sealed partial class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentCheermote
	{
        [JsonPropertyName("bits")]
        public int? Bits = 0;

        [JsonPropertyName("prefix")]
        public string Prefix = string.Empty;

        [JsonPropertyName("tier")]
        public int? Tier = 0;
	}
}