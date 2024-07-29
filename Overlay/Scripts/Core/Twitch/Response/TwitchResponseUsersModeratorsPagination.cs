
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersModeratorsPagination
    {
        [JsonPropertyName(name: "cursor")]
        public string Cursor { get; set; } = string.Empty;
    }
}