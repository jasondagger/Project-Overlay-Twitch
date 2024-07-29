
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseAlbum
    {
        [JsonPropertyName(name: "album_type")]
        public string AlbumnType { get; set; } = string.Empty;

        [JsonPropertyName(name: "total_tracks")]
        public int TotalTracks { get; set; } = 0;

        [JsonPropertyName(name: "available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName(name: "external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName(name: "release_date_precision")]
        public string ReleaseDatePrecision { get; set; } = string.Empty;

        [JsonPropertyName(name: "restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName(name: "artists")]
        public SpotifyResponseSimplifiedArtist[] Artists { get; set; } = null;
    }
}