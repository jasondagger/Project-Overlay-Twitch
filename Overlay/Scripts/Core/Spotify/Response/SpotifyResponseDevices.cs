
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseDevices
    {
        [JsonPropertyName("devices")]
        public SpotifyResponseDevice[] Devices { get; set; } = null;
    }
}