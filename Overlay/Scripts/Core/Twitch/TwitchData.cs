
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchData
    {
        [JsonPropertyName("AccountAccessToken")]
        public string AccountAccessToken { get; set; } = string.Empty;

        [JsonPropertyName("AccountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonPropertyName("AccountUsername")]
        public string AccountUserName { get; set; } = string.Empty;

        [JsonPropertyName("BotAccessToken")]
        public string BotAccessToken { get; set; } = string.Empty;

        [JsonPropertyName("BotUsername")]
        public string BotUsername { get; set; } = string.Empty;

        [JsonPropertyName("ClientId")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [JsonPropertyName("TwitchChannel")]
        public string TwitchChannel { get; set; } = string.Empty;
    }
}