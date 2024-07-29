
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItemEpisodes
    {
        [JsonPropertyName(name: "access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}