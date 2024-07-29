
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseShow
    {
        [JsonPropertyName(name: "available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName(name: "copyrights")]
        public SpotifyResponseCopyrights Copyrights { get; set; } = null;

        [JsonPropertyName(name: "description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName(name: "explicit")]
        public bool Explicit { get; set; } = false;

        [JsonPropertyName(name: "external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "html_description")]
        public string HtmlDescription { get; set; } = string.Empty;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName(name: "is_externally_hosted")]
        public bool IsExternallyHosted { get; set; } = false;

        [JsonPropertyName(name: "language")]
        public string[] Languages { get; set; } = null;

        [JsonPropertyName(name: "media_type")]
        public string MediaType { get; set; } = string.Empty;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "publisher")]
        public string Publisher { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName(name: "total_episodes")]
        public int TotalEpisodes { get; set; } = 0;
    }
}