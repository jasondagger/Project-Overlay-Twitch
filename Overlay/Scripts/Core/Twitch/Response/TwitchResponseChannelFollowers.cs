
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseChannelFollowers
    {
        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName("data")]
        public TwitchResponseChannelFollowersData[] Data { get; set; } = null;

        [JsonPropertyName("pagination")]
        public TwitchResponseChannelFollowersPagination Pagination { get; set; } = null;
    }
}