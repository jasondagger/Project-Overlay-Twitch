
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseUsers
    {
        [JsonPropertyName("data")]
        public TwitchResponseUser[] Data { get; set; } = null;
    }
}