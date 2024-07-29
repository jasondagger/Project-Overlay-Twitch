
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItemAlbums
    {
        [JsonPropertyName(name: "access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}