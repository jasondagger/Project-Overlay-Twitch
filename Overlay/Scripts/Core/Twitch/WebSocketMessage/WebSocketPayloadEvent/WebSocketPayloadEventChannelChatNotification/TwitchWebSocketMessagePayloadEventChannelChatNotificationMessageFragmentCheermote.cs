
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed partial class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentCheermote
	{
        [JsonPropertyName(name: "bits")]
        public int? Bits { get; set; } = 0;

        [JsonPropertyName(name: "prefix")]
        public string Prefix { get; set; } = string.Empty;

        [JsonPropertyName(name: "tier")]
        public int? Tier { get; set; } = 0;
	}
}