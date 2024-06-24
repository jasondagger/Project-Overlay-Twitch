
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseBadges
    {
        [JsonPropertyName("data")]
        public TwitchResponseBadgeData[] Data { get; set; } = null;
    }
}