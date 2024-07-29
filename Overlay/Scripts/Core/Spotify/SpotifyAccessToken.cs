
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyAccessToken
    {
        [JsonPropertyName(name: "AccessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "RefreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName(name: "ExpireTime")]
        public string ExpireTime { get; set; } = string.Empty;

        public SpotifyAccessToken(
            string accessToken,
            string refreshToken,
            string expireTime
        )
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.ExpireTime = expireTime;
        }
    }
}