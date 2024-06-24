
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseBadgeData
    {
        [JsonPropertyName("set_id")]
        public string SetId { get; set; } = string.Empty;

        [JsonPropertyName("versions")]
        public TwitchResponseBadgeDataVersions[] Versions { get; set; } = null;
    }
}