
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseAlbum
    {
        [JsonPropertyName("album_type")]
        public string AlbumnType { get; set; } = string.Empty;

        [JsonPropertyName("total_tracks")]
        public int TotalTracks { get; set; } = 0;

        [JsonPropertyName("available_markets")]
        public string[] AvailableMarkets { get; set; } = null;

        [JsonPropertyName("external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; set; } = string.Empty;

        [JsonPropertyName("restrictions")]
        public SpotifyResponseRestrictions Restrictions { get; set; } = null;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("artists")]
        public SpotifyResponseSimplifiedArtist[] Artists { get; set; } = null;
    }
}