
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseUsersSubscribersPagination
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; } = string.Empty;
    }
}