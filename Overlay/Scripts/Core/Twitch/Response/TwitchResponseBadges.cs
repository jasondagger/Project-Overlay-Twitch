
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseBadges
    {
        [JsonPropertyName("data")]
        public TwitchResponseBadgeData[] Data { get; set; } = null;
    }
}