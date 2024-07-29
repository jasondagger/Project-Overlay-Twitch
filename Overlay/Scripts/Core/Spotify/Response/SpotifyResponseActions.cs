
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseActions
    {
        [JsonPropertyName(name: "interrupting_playback")]
        public bool InterruptingPlayback { get; set; } = false;

        [JsonPropertyName(name: "pausing")]
        public bool Pausing { get; set; } = false;

        [JsonPropertyName(name: "resuming")]
        public bool Resuming { get; set; } = false;

        [JsonPropertyName(name: "seeking")]
        public bool Seeking { get; set; } = false;

        [JsonPropertyName(name: "skipping_next")]
        public bool SkippingNext { get; set; } = false;

        [JsonPropertyName(name: "skipping_prev")]
        public bool SkippingPrevious { get; set; } = false;

        [JsonPropertyName(name: "toggling_repeat_context")]
        public bool TogglingRepeatContext { get; set; } = false;

        [JsonPropertyName(name: "toggling_shuffle")]
        public bool TogglingShuffle { get; set; } = false;

        [JsonPropertyName(name: "toggling_repeat_track")]
        public bool TogglingRepeatTrack { get; set; } = false;

        [JsonPropertyName(name: "transferring_playback")]
        public bool TransferringPlayback { get; set; } = false;
    }
}