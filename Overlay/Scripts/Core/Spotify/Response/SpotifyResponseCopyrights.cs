
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseCopyrights
    {
        [JsonPropertyName(name: "text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;
    }
}