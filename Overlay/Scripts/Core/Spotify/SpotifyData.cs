
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyData
    {
        [JsonPropertyName("ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [JsonPropertyName("ClientId")]
        public string ClientId { get; set; } = string.Empty;
    }
}