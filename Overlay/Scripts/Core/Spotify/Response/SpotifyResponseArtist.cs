
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseArtist
    {
        [JsonPropertyName("external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName("followers")]
        public SpotifyResponseFollowers Followers { get; set; } = null;

        [JsonPropertyName("genres")]
        public string[] Genres { get; set; } = null;

        [JsonPropertyName("href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; } = 0;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}