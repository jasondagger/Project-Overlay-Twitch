
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseExternalIds
    {
        [JsonPropertyName("isrc")]
        public string ISRC { get; set; } = string.Empty;

        [JsonPropertyName("ean")]
        public string EAN { get; set; } = string.Empty;

        [JsonPropertyName("upc")]
        public string UPC { get; set; } = string.Empty;
    }
}