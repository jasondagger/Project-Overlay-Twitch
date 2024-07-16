
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseCurrentTrack
    {
        [JsonPropertyName("actions")]
        public SpotifyResponseActions Actions { get; set; } = null;

        [JsonPropertyName("context")]
        public SpotifyResponseContext Context { get; set; } = null;

        [JsonPropertyName("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; } = string.Empty;

        [JsonPropertyName("device")]
        public SpotifyResponseDevice Device { get; set; } = null;

        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; } = false;

        [JsonPropertyName("progress_ms")]
        public int ProgressMS { get; set; } = 0;

        [JsonPropertyName("repeat_state")]
        public string RepeatState { get; set; } = string.Empty;

        [JsonPropertyName("shuffle_state")]
        public bool ShuffleState { get; set; } = false;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = 0L;

        [JsonPropertyName("item")]
        public SpotifyResponseTrack Track { get; set; } = null;
    }
}