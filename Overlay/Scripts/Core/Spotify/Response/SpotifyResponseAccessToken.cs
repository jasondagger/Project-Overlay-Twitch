
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseAccessToken
    {
        [JsonPropertyName(name: "access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "expires_in")]
        public int ExpiresIn { get; set; } = 0;

        [JsonPropertyName(name: "refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName(name: "token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}