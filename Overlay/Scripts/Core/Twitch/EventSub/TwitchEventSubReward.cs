
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchEventSubReward
    {
        [JsonPropertyName(name: "cost")]
        public int? Cost { get; set; } = 0;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName(name: "title")]
        public string Title { get; set; } = string.Empty;
    }
}