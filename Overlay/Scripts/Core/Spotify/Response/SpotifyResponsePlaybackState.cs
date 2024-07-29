
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponsePlaybackState
    {
        [JsonPropertyName(name: "device")]
        public SpotifyResponseDevice Device { get; set; } = null;

        [JsonPropertyName(name: "repeat_state")]
        public string RepeatState { get; set; } = string.Empty;

        [JsonPropertyName(name: "shuffle_state")]
        public bool ShuffleState { get; set; } = false;

        [JsonPropertyName(name: "timestamp")]
        public int TimeStamp { get; set; } = 0;

        [JsonPropertyName(name: "progress_ms")]
        public int ProgressInMilliseconds { get; set; } = 0;

        [JsonPropertyName(name: "is_playing")]
        public bool IsPlaying { get; set; } = false;

        [JsonPropertyName(name: "currently_playing_type")]
        public string CurrentlyPlayingType { get; set; } = string.Empty;
    }
}