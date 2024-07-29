
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseBadgeDataVersions
    {
        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "image_url_1x")]
        public string ImageUrl1x { get; set; } = string.Empty;

        [JsonPropertyName(name: "image_url_2x")]
        public string ImageUrl2x { get; set; } = string.Empty;

        [JsonPropertyName(name: "image_url_4x")]
        public string ImageUrl4x { get; set; } = string.Empty;

        [JsonPropertyName(name: "title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName(name: "description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName(name: "click_action")]
        public string ClickAction { get; set; } = string.Empty;

        [JsonPropertyName(name: "click_url")]
        public string ClickUrl { get; set; } = string.Empty;
    }
}