
using System.Collections.Generic;

namespace Overlay
{
    public sealed class SpotifyTwitchData
    {
        public string ArtistName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string OnScreenMessage { get; set; } = string.Empty;
        public int QueuePosition { get; set; } = 0;
        public string SearchParameters { get; set; } = string.Empty;
        public SpotifyTwitchDataRequestType SpotifyTwitchDataRequestType { get; set; } = SpotifyTwitchDataRequestType.CurrentTrack;
        public Queue<SpotifyUserTrackData> SpotifyQueuedUserTracks { get; set; } = null;
        public string TrackId { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string TwitchChatMessageId { get; set; } = string.Empty;
        public string TwitchUserName { get; set; } = string.Empty;

        public SpotifyTwitchData(
            SpotifyTwitchDataRequestType spotifyTwitchDataRequestType
        )
        {
            this.SpotifyTwitchDataRequestType = spotifyTwitchDataRequestType;
        }
    }
}