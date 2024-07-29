
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseExternalUrls
    {
        [JsonPropertyName(name: "spotify")]
        public string Spotify { get; set; } = string.Empty;
    }
}