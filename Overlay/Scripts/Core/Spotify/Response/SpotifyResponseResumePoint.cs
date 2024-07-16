
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseResumePoint
    {
        [JsonPropertyName("fully_played")]
        public bool FullyPlayed { get; set; } = false;

        [JsonPropertyName("resume_position_ms")]
        public string ResumePositionMS { get; set; } = string.Empty;
    }
}