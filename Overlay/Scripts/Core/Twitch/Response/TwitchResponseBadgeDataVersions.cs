
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseBadgeDataVersions
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("image_url_1x")]
        public string ImageUrl1x { get; set; } = string.Empty;

        [JsonPropertyName("image_url_2x")]
        public string ImageUrl2x { get; set; } = string.Empty;

        [JsonPropertyName("image_url_4x")]
        public string ImageUrl4x { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("click_action")]
        public string ClickAction { get; set; } = string.Empty;

        [JsonPropertyName("click_url")]
        public string ClickUrl { get; set; } = string.Empty;
    }
}