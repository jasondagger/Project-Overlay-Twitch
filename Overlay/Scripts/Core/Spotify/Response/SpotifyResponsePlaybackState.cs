
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponsePlaybackState
    {
        [JsonPropertyName("device")]
        public SpotifyResponseDevice Device { get; set; } = null;

        [JsonPropertyName("repeat_state")]
        public string RepeatState { get; set; } = string.Empty;

        [JsonPropertyName("shuffle_state")]
        public bool ShuffleState { get; set; } = false;

        [JsonPropertyName("timestamp")]
        public int TimeStamp { get; set; } = 0;

        [JsonPropertyName("progress_ms")]
        public int ProgressInMilliseconds { get; set; } = 0;

        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; } = false;

        [JsonPropertyName("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; } = string.Empty;
    }
}