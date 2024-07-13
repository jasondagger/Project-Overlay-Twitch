
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItemTracks
    {
        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public SpotifyResponseTrack[] Items { get; set; } = null;

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 0;

        [JsonPropertyName("next")]
        public string Next { get; set; } = string.Empty;

        [JsonPropertyName("offset")]
        public int Offset { get; set; } = 0;

        [JsonPropertyName("previous")]
        public string Previous { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;
    }
}