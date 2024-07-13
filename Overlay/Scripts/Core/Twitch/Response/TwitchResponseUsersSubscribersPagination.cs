
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersSubscribersPagination
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; } = string.Empty;
    }
}