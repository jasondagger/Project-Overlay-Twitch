
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataImage
    {
        [JsonPropertyName("url_1x")]
        public string Url1x { get; set; } = string.Empty;

        [JsonPropertyName("url_2x")]
        public string Url2x { get; set; } = string.Empty;

        [JsonPropertyName("url_4x")]
        public string Url4x { get; set; } = string.Empty;
    }
}