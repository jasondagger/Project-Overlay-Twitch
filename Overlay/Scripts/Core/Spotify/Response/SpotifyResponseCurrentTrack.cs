
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseCurrentTrack
    {
        [JsonPropertyName(name: "actions")]
        public SpotifyResponseActions Actions { get; set; } = null;

        [JsonPropertyName(name: "context")]
        public SpotifyResponseContext Context { get; set; } = null;

        [JsonPropertyName(name: "currently_playing_type")]
        public string CurrentlyPlayingType { get; set; } = string.Empty;

        [JsonPropertyName(name: "device")]
        public SpotifyResponseDevice Device { get; set; } = null;

        [JsonPropertyName(name: "is_playing")]
        public bool IsPlaying { get; set; } = false;

        [JsonPropertyName(name: "progress_ms")]
        public int ProgressMS { get; set; } = 0;

        [JsonPropertyName(name: "repeat_state")]
        public string RepeatState { get; set; } = string.Empty;

        [JsonPropertyName(name: "shuffle_state")]
        public bool ShuffleState { get; set; } = false;

        [JsonPropertyName(name: "timestamp")]
        public long Timestamp { get; set; } = 0L;

        [JsonPropertyName(name: "item")]
        public SpotifyResponseTrack Track { get; set; } = null;
    }
}