
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchGlobalData
    {
        [JsonPropertyName(name: "AccountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonPropertyName(name: "AccountUserName")]
        public string AccountUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "BotUserName")]
        public string BotUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "ClientId")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName(name: "ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [JsonPropertyName(name: "TwitchChannel")]
        public string TwitchChannel { get; set; } = string.Empty;
    }
}