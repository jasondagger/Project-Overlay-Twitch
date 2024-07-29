
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyData
    {
        [JsonPropertyName(name: "ClientSecret")]
        public string ClientSecret { get; set; } = string.Empty;

        [JsonPropertyName(name: "ClientId")]
        public string ClientId { get; set; } = string.Empty;
    }
}