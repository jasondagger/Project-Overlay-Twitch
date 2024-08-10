
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseAccessToken
    {
        [JsonPropertyName(name: "access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "expires_in")]
        public int ExpiresIn { get; set; } = 0;

        [JsonPropertyName(name: "refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "scope")]
        public string[] Scope { get; set; } = null;

        [JsonPropertyName(name: "token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}