
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchData
    {
        [JsonPropertyName(name: "AccountAccessToken")]
        public string AccountAccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "AccountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonPropertyName(name: "AccountUsername")]
        public string AccountUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "BotAccessToken")]
        public string BotAccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "BotUsername")]
        public string BotUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "ClientId")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName(name: "ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [JsonPropertyName(name: "TwitchChannel")]
        public string TwitchChannel { get; set; } = string.Empty;
    }
}