
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItem
    {
        [JsonPropertyName("albums")]
        public SpotifyResponseSearchItemAlbums Albums { get; set; } = null;

        [JsonPropertyName("artists")]
        public SpotifyResponseSearchItemArtists Artists { get; set; } = null;

        [JsonPropertyName("audiobooks")]
        public SpotifyResponseSearchItemAudiobooks Audiobooks { get; set; } = null;

        [JsonPropertyName("episodes")]
        public SpotifyResponseSearchItemEpisodes Episodes { get; set; } = null;

        [JsonPropertyName("playlists")]
        public SpotifyResponseSearchItemPlaylists Playlists { get; set; } = null;

        [JsonPropertyName("shows")]
        public SpotifyResponseSearchItemShows Shows { get; set; } = null;

        [JsonPropertyName("tracks")]
        public SpotifyResponseSearchItemTracks Tracks { get; set; } = null;
    }
}