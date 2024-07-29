
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataImage
    {
        [JsonPropertyName(name: "url_1x")]
        public string Url1x { get; set; } = string.Empty;

        [JsonPropertyName(name: "url_2x")]
        public string Url2x { get; set; } = string.Empty;

        [JsonPropertyName(name: "url_4x")]
        public string Url4x { get; set; } = string.Empty;
    }
}