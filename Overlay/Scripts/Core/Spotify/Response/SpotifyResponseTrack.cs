
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseTrack
    {
        [JsonPropertyName("album")]
        public SpotifyResponseAlbum Album { get; set; } = null;

        [JsonPropertyName("artists")]
        public SpotifyResponseArtist[] Artists { get; set; } = null;

        [JsonPropertyName("available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName("disc_number")]
        public int DiscNumber { get; set; } = 0;

        [JsonPropertyName("duration_ms")]
        public int DurationInMilliseconds { get; set; } = 0;

        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; } = false;

        [JsonPropertyName("external_ids")]
        public SpotifyResponseExternalIds ExternalIds { get; set; } = null;

        [JsonPropertyName("external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; } = false;

        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; } = false;

        [JsonPropertyName("linked_from")]
        public SpotifyResponseLinkedFrom LinkedFrom { get; set; } = null;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; } = 0;

        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; } = string.Empty;

        [JsonPropertyName("restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName("track_number")]
        public int TrackNumber { get; set; } = 0;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}