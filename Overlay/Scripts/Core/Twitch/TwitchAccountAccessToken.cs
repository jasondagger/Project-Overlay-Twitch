
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchAccountAccessToken
    {
        [JsonPropertyName(name: "AccessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "RefreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}