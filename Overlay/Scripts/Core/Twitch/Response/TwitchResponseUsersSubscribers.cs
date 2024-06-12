
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseUsersSubscribers
    {
        [JsonPropertyName("data")]
        public TwitchResponseUsersSubscribersData[] Data { get; set; } = null;

        [JsonPropertyName("pagination")]
        public TwitchResponseUsersSubscribersPagination Pagination { get; set; } = null;

        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName("points")]
        public int Points { get; set; } = 0;
    }
}