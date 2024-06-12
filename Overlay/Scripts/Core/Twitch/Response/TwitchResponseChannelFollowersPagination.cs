
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseChannelFollowersPagination
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; } = string.Empty;
    }
}