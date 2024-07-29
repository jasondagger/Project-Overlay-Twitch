
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersSubscribers
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseUsersSubscribersData[] Data { get; set; } = null;

        [JsonPropertyName(name: "pagination")]
        public TwitchResponseUsersSubscribersPagination Pagination { get; set; } = null;

        [JsonPropertyName(name: "total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName(name: "points")]
        public int Points { get; set; } = 0;
    }
}