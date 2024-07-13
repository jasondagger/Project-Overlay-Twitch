
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseImage
    {
        [JsonPropertyName("height")]
        public int Height { get; set; } = 0;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; } = 0;
    }
}