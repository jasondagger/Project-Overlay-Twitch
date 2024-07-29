
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseContext
    {
        [JsonPropertyName(name: "external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;
    }
}