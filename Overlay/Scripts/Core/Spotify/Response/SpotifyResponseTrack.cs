
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseTrack
    {
        [JsonPropertyName(name: "album")]
        public SpotifyResponseAlbum Album { get; set; } = null;

        [JsonPropertyName(name: "artists")]
        public SpotifyResponseArtist[] Artists { get; set; } = null;

        [JsonPropertyName(name: "available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName(name: "disc_number")]
        public int DiscNumber { get; set; } = 0;

        [JsonPropertyName(name: "duration_ms")]
        public int DurationInMilliseconds { get; set; } = 0;

        [JsonPropertyName(name: "explicit")]
        public bool Explicit { get; set; } = false;

        [JsonPropertyName(name: "external_ids")]
        public SpotifyResponseExternalIds ExternalIds { get; set; } = null;

        [JsonPropertyName(name: "external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "is_local")]
        public bool IsLocal { get; set; } = false;

        [JsonPropertyName(name: "is_playable")]
        public bool IsPlayable { get; set; } = false;

        [JsonPropertyName(name: "linked_from")]
        public SpotifyResponseLinkedFrom LinkedFrom { get; set; } = null;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "popularity")]
        public int Popularity { get; set; } = 0;

        [JsonPropertyName(name: "preview_url")]
        public string PreviewUrl { get; set; } = string.Empty;

        [JsonPropertyName(name: "restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName(name: "track_number")]
        public int TrackNumber { get; set; } = 0;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;
    }
}