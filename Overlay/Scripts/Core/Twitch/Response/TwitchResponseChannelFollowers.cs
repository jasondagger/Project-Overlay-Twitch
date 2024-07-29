
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseChannelFollowers
    {
        [JsonPropertyName(name: "total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName(name: "data")]
        public TwitchResponseChannelFollowersData[] Data { get; set; } = null;

        [JsonPropertyName(name: "pagination")]
        public TwitchResponseChannelFollowersPagination Pagination { get; set; } = null;
    }
}