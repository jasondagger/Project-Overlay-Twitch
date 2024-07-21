
namespace Overlay
{
    public enum SpotifyTwitchDataRequestType : uint
    {
        CurrentTrack = 0u,
        TrackQueueBySearchTerms,
        TrackQueueByTrackId,
        TrackSkip,
        UserTrackQueue,
    }
}