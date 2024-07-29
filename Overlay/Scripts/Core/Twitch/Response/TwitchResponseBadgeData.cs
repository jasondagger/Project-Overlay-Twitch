
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseBadgeData
    {
        [JsonPropertyName(name: "set_id")]
        public string SetId { get; set; } = string.Empty;

        [JsonPropertyName(name: "versions")]
        public TwitchResponseBadgeDataVersions[] Versions { get; set; } = null;
    }
}