
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseDevice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = false;

        [JsonPropertyName("is_private_session")]
        public bool IsPrivateSession { get; set; } = false;

        [JsonPropertyName("is_restricted")]
        public bool IsRestricted { get; set; } = false;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("volume_percent")]
        public int VolumePercent { get; set; } = 0;

        [JsonPropertyName("supports_volume")]
        public bool SupportsVolume { get; set; } = false;
    }
}