
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseQueue
    {
        [JsonPropertyName("currently_playing")]
        public SpotifyResponseTrack CurrentlyPlaying { get; set; } = null;

        [JsonPropertyName("queue")]
        public SpotifyResponseTrack[] Queue { get; set; } = null;
    }
}