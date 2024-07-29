
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseEpisode
    {
        [JsonPropertyName(name: "audio_preview_url")]
        public string AudioPreviewUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName(name: "duration_ms")]
        public int DurationMS { get; set; } = 0;

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

        [JsonPropertyName(name: "is_playable")]
        public bool IsPlayable { get; set; } = false;

        [JsonPropertyName(name: "language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName(name: "languages")]
        public string[] Languages { get; set; } = null;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName(name: "release_date_precision")]
        public string ReleaseDatePrecision { get; set; } = string.Empty;

        [JsonPropertyName(name: "restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName(name: "resume_point")]
        public SpotifyResponseResumePoint ResumePoint { get; set; } = null;

        [JsonPropertyName(name: "show")]
        public SpotifyResponseShow Show { get; set; } = null;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;
    }
}