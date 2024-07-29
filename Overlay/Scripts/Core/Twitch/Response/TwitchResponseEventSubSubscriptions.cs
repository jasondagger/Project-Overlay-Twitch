
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseEventSubSubscriptions
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseEventSubSubscriptionsData[] Data { get; set; } = null;

        [JsonPropertyName(name: "total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName(name: "total_cost")]
        public int TotalCost { get; set; } = 0;

        [JsonPropertyName(name: "max_total_cost")]
        public int MaxTotalCost { get; set; } = 0;

        [JsonPropertyName(name: "pagination")]
        public TwitchResponseEventSubSubscriptionsPagination Pagination { get; set; } = null;
    }
}