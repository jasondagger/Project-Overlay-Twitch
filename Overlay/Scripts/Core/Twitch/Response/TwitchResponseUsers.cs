
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsers
    {
        [JsonPropertyName("data")]
        public TwitchResponseUser[] Data { get; set; } = null;
    }
}