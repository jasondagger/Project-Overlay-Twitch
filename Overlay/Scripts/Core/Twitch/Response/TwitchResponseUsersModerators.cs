
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersModerators
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseUsersModeratorsData[] Data { get; set; } = null;

        [JsonPropertyName(name: "pagination")]
        public TwitchResponseUsersModeratorsPagination Pagination { get; set; } = null;
    }
}