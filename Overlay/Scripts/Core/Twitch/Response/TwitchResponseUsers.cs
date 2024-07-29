
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsers
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseUser[] Data { get; set; } = null;
    }
}