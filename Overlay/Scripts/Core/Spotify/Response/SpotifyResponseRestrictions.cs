
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseRestrictions
    {
        [JsonPropertyName(name: "reason")]
        public string Reason { get; set; } = string.Empty;
    }
}