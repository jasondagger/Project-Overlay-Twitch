
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItemTracks
    {
        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "items")]
        public SpotifyResponseTrack[] Items { get; set; } = null;

        [JsonPropertyName(name: "limit")]
        public int Limit { get; set; } = 0;

        [JsonPropertyName(name: "next")]
        public string Next { get; set; } = string.Empty;

        [JsonPropertyName(name: "offset")]
        public int Offset { get; set; } = 0;

        [JsonPropertyName(name: "previous")]
        public string Previous { get; set; } = string.Empty;

        [JsonPropertyName(name: "total")]
        public int Total { get; set; } = 0;
    }
}