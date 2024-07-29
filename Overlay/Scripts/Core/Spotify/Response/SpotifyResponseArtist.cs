
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseArtist
    {
        [JsonPropertyName(name: "external_urls")]
        public SpotifyResponseExternalUrls ExternalUrls { get; set; } = null;

        [JsonPropertyName(name: "followers")]
        public SpotifyResponseFollowers Followers { get; set; } = null;

        [JsonPropertyName(name: "genres")]
        public string[] Genres { get; set; } = null;

        [JsonPropertyName(name: "href")]
        public string HRef { get; set; } = string.Empty;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "images")]
        public SpotifyResponseImage[] Images { get; set; } = null;

        [JsonPropertyName(name: "name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(name: "popularity")]
        public int Popularity { get; set; } = 0;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName(name: "uri")]
        public string Uri { get; set; } = string.Empty;
    }
}