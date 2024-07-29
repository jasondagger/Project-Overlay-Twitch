
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItem
    {
        [JsonPropertyName(name: "albums")]
        public SpotifyResponseSearchItemAlbums Albums { get; set; } = null;

        [JsonPropertyName(name: "artists")]
        public SpotifyResponseSearchItemArtists Artists { get; set; } = null;

        [JsonPropertyName(name: "audiobooks")]
        public SpotifyResponseSearchItemAudiobooks Audiobooks { get; set; } = null;

        [JsonPropertyName(name: "episodes")]
        public SpotifyResponseSearchItemEpisodes Episodes { get; set; } = null;

        [JsonPropertyName(name: "playlists")]
        public SpotifyResponseSearchItemPlaylists Playlists { get; set; } = null;

        [JsonPropertyName(name: "shows")]
        public SpotifyResponseSearchItemShows Shows { get; set; } = null;

        [JsonPropertyName(name: "tracks")]
        public SpotifyResponseSearchItemTracks Tracks { get; set; } = null;
    }
}