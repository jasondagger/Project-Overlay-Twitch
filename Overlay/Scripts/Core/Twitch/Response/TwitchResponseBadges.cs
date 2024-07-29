
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseBadges
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseBadgeData[] Data { get; set; } = null;
    }
}