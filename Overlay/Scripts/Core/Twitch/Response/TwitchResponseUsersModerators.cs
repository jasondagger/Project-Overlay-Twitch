
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersModerators
    {
        [JsonPropertyName("data")]
        public TwitchResponseUsersModeratorsData[] Data { get; set; } = null;

        [JsonPropertyName("pagination")]
        public TwitchResponseUsersModeratorsPagination Pagination { get; set; } = null;
    }
}