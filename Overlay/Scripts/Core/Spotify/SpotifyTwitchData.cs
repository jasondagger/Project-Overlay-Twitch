
namespace Overlay
{
    public sealed class SpotifyTwitchData
    {
        public string ArtistName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string SearchParameters { get; set; } = string.Empty;
        public SpotifyTwitchDataRequestType SpotifyTwitchDataRequestType { get; set; } = SpotifyTwitchDataRequestType.CurrentTrack;
        public string TrackName { get; set; } = string.Empty;
        public string TwitchChatMessageId { get; set; } = string.Empty;

        public SpotifyTwitchData(
            SpotifyTwitchDataRequestType spotifyTwitchDataRequestType,
            string twitchChatMessageId
        )
        {
            this.SpotifyTwitchDataRequestType = spotifyTwitchDataRequestType;
            this.TwitchChatMessageId = twitchChatMessageId;
        }
    }
}