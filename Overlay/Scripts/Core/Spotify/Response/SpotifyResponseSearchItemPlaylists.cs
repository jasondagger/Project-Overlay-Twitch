
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class SpotifyResponseSearchItemPlaylists
    {
        [JsonPropertyName(name: "access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}