
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSimplifiedArtist
    {
        [JsonPropertyName("external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}