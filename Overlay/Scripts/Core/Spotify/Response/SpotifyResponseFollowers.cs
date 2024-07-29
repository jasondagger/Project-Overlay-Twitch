
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseFollowers
    {
        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "total")]
        public int Total { get; set; } = 0;
    }
}