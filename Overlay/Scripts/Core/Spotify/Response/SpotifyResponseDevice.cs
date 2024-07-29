
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseDevice
    {
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "is_active")]
        public bool IsActive { get; set; } = false;

        [JsonPropertyName(name: "is_private_session")]
        public bool IsPrivateSession { get; set; } = false;

        [JsonPropertyName(name: "is_restricted")]
        public bool IsRestricted { get; set; } = false;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "volume_percent")]
        public int VolumePercent { get; set; } = 0;

        [JsonPropertyName(name: "supports_volume")]
        public bool SupportsVolume { get; set; } = false;
    }
}