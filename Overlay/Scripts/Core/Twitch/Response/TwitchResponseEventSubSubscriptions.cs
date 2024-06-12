
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseEventSubSubscriptions
    {
        [JsonPropertyName("data")]
        public TwitchResponseEventSubSubscriptionsData[] Data { get; set; } = null;

        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName("total_cost")]
        public int TotalCost { get; set; } = 0;

        [JsonPropertyName("max_total_cost")]
        public int MaxTotalCost { get; set; } = 0;

        [JsonPropertyName("pagination")]
        public TwitchResponseEventSubSubscriptionsPagination Pagination { get; set; } = null;
    }
}