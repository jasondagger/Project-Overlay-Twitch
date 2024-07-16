
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseEpisode
    {
        [JsonPropertyName("audio_preview_url")]
        public string AudioPreviewUrl { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("duration_ms")]
        public int DurationMS { get; set; } = 0;

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

        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; } = false;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("languages")]
        public string[] Languages { get; set; } = null;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; set; } = string.Empty;

        [JsonPropertyName("restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName("resume_point")]
        public SpotifyResponseResumePoint ResumePoint { get; set; } = null;

        [JsonPropertyName("show")]
        public SpotifyResponseShow Show { get; set; } = null;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}