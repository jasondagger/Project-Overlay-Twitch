
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseShow
    {
        [JsonPropertyName("available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName("copyrights")]
        public SpotifyResponseCopyrights Copyrights { get; set; } = null;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; } = false;

        [JsonPropertyName("external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("html_description")]
        public string HtmlDescription { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName("is_externally_hosted")]
        public bool IsExternallyHosted { get; set; } = false;

        [JsonPropertyName("language")]
        public string[] Languages { get; set; } = null;

        [JsonPropertyName("media_type")]
        public string MediaType { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("publisher")]
        public string Publisher { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("total_episodes")]
        public int TotalEpisodes { get; set; } = 0;
    }
}