
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseExternalUrls
    {
        [JsonPropertyName("spotify")]
        public string Spotify { get; set; } = string.Empty;
    }
}