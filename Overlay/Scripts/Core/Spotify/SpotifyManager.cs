
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using static Godot.HttpClient;
    using NodeType = NodeDirectory.NodeType;
    using RequiredFileType = ApplicationManager.RequiredFileType;

    public sealed partial class SpotifyManager : Node
    {
        public Action<SpotifyTwitchData> CurrentTrackRetrieved = null;
        public Action<SpotifyTwitchData> Errored = null;
        public Action<SpotifyTwitchData> TrackQueuedCompleted = null;
        public Action<SpotifyTwitchData> TrackSkipped = null;
        public Action<SpotifyTwitchData> UserTrackQueueRetrieveCompleted = null;
        public Action<SpotifyTwitchData> UserTrackQueueRetrieveFailed = null;

        public override void _Process(
            double delta    
        )
        {
            ProcessHttpRequests();
        }

        public override void _Ready()
        {
            RetrieveResources();
        }

        public static string ParseSpotifyUriForTrackId(
            string uri
        )
        {
            var count = uri.StartsWith(
                value: c_uriHttpPrefixInsecure
            ) ? c_uriSpotifyTrackInsecureLength : c_uriSpotifyTrackSecureLength;

            var uriSplit = uri.Split(
                separator: '?'
            );
            var trackId = uriSplit[0].Remove(
                startIndex: 0,
                count: count
            );
            return trackId;
        }

        public void QueueRequestCurrentTrack(
            string twitchUserName,
            string twitchChatMessageId
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.CurrentTrack
            )
            {
                TwitchChatMessageId = twitchChatMessageId,
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        public void QueueRequestSkipTrack(
            string twitchUserName,
            string twitchChatMessageId
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackSkip
            )
            {
                TwitchChatMessageId = twitchChatMessageId,
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        public void QueueRequestTrackQueueBySeachTerms(
            string twitchUserName,
            string twitchChatMessageId,
            string searchParameters
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackQueueBySearchTerms
            )
            {
                SearchParameters = searchParameters,
                TwitchChatMessageId = twitchChatMessageId,
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        public void QueueRequestTrackQueueByTrackId(
            string twitchUserName,
            string twitchChatMessageId,
            string trackId
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackQueueByTrackId
            )
            {
                TrackId = trackId,
                TwitchChatMessageId = twitchChatMessageId,
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        public void QueueRequestUserTrackQueue(
            string twitchUserName,
            string twitchChatMessageId
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.UserTrackQueue
            )
            {
                TwitchChatMessageId = twitchChatMessageId,
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        public static bool StartsWithValidSpotifyUri(
            string uri    
        )
        {
            if (
                uri.StartsWith(
                    value: c_uriHttpPrefixInsecure
                )
            )
            {
                uri = uri.Replace(
                    what: c_uriHttpPrefixInsecure,
                    forwhat: c_uriHttpPrefixSecure
                );
            }

            return uri.StartsWith(
                value: c_uriSpotifyTrackSecure    
            ) is true;
        }

        private const int c_accessTokenRefreshTimeInMilliseconds = 3600000;
        private const string c_authorizationCode = "AQCsZ-HvlDJyhOOJ52TZsvnNZQAibiQXupDEjdTrK497FW8oTXj1q8P3vLGm89yP8JIfZkIR_jO3s7Rn63RPRW6xC9ocRWPPal4xqEaBLiglYZBIS3TL0XJ84rI-2axiODaFH27kLnLGyd38hBBzE9aieNBdCi0_B5B1i-iizw4eswPVp84qWyarLLea9u6N4fj3QZUs_9TJ6od-zflrYl34oy4Q0iEpvdy9YbJwd2vsNxf8shpcE2fsHD5zA4wGw60znkZRkGAtqzkg5ZFbaOVCIXco";

        private const string c_redirectUri = "http://localhost:8888/callback";
        private const string c_uriAPI = "https://api.spotify.com/v1";
        private const string c_uriHttpPrefixSecure = "https://";
        private const string c_uriHttpPrefixInsecure = "http://";
        private const string c_uriAccessToken = "https://accounts.spotify.com/api/token";
        private const string c_uriSpotifyTrackSecure = "https://open.spotify.com/track/";
        private const string c_uriSpotifyTrackInsecure = "http://open.spotify.com/track/";
        private const string c_userAccessScopes = "user-modify-playback-state user-read-currently-playing user-read-playback-state";

        private static readonly int c_uriSpotifyTrackSecureLength = c_uriSpotifyTrackSecure.Length;
        private static readonly int c_uriSpotifyTrackInsecureLength = c_uriSpotifyTrackInsecure.Length;

        private readonly Queue<SpotifyTwitchData> m_currentSpotifyTwitchDatas = new();
        private readonly Queue<SpotifyUserTrackData> m_currentSpotifyQueuedUserTrackDatas = new();
        private readonly object m_currentSpotifyTwitchDatasLock = new();

        private HttpManager m_httpManager = null;
        private SpotifyAccessToken m_spotifyAccessToken = null;
        private SpotifyData m_spotifyData = null;
        private SpotifyResponseTrack[] m_spotifyTrackQueue = null;
        private SpotifyTwitchData m_spotifyTwitchData = null;

        private void BindTwitchChannelPointRewards()
        {
            var twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchChannelPointRewardsManager
                )
            );
            
            twitchChannelPointRewardsManager.CommandRequestSongClaimed += OnChannelPointRewardsRedeemedRequestSong;
            twitchChannelPointRewardsManager.CommandSkipSongClaimed += OnChannelPointRewardsRedeemedSkipSong;
        }

        private static bool ContainsAValidSpotifyUri(
            string uri
        )
        {
            if (
                uri.Contains(
                    value: c_uriHttpPrefixInsecure
                )
            )
            {
                uri = uri.Replace(
                    what: c_uriHttpPrefixInsecure,
                    forwhat: c_uriHttpPrefixSecure
                );
            }

            return uri.Contains(
                value: c_uriSpotifyTrackSecure    
            ) is true;
        }

        private bool IsAccessTokenExpired()
        {
            return DateTime.Compare(
                t1: DateTime.UtcNow,
                t2: DateTime.Parse(
                    s: m_spotifyAccessToken.ExpireTime
                )
            ) >= 0;
        }

        private void OnChannelPointRewardsRedeemedRequestSong(
            string searchText,
            string twitchUserName
        )
        {
            SpotifyTwitchData spotifyTwitchData;
            if (
                ContainsAValidSpotifyUri(
					uri: searchText
                ) is true
			)
			{
				var trackId = ParseSearchTextForTrackId(
					searchText: searchText
                );
                spotifyTwitchData = new SpotifyTwitchData(
                    spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackQueueByTrackId
                )
                {
                    TwitchUserName = twitchUserName,
                    TrackId = trackId,
                };
			}
			else
			{
                spotifyTwitchData = new SpotifyTwitchData(
                    spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackQueueBySearchTerms
                )
                {
                    TwitchUserName = twitchUserName,
                    SearchParameters = searchText,
                };
            }

            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        private void OnChannelPointRewardsRedeemedSkipSong(
            string twitchUserName    
        )
        {
            var spotifyTwitchData = new SpotifyTwitchData(
                spotifyTwitchDataRequestType: SpotifyTwitchDataRequestType.TrackSkip
            )
            {
                TwitchUserName = twitchUserName,
            };
            lock (m_currentSpotifyTwitchDatasLock)
            {
                m_currentSpotifyTwitchDatas.Enqueue(
                    item: spotifyTwitchData
                );
            }
        }

        private void OnRequestAccessTokenCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAccessTokenCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                WriteAccessToken(
                    response: JsonSerializer.Deserialize<SpotifyResponseAccessToken>(
                        json: Encoding.UTF8.GetString(
                            bytes: body,
                            index: 0,
                            count: body.Length
                        )
                    ),
                    wasRefreshed: false
                );

                QueueRequestAccessTokenWithRefreshToken();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAccessTokenCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnRequestAccessTokenWithRefreshTokenCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAccessTokenWithRefreshTokenCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                WriteAccessToken(
                    response: JsonSerializer.Deserialize<SpotifyResponseAccessToken>(
                        json: Encoding.UTF8.GetString(
                            bytes: body,
                            index: 0,
                            count: body.Length
                        )
                    ),
                    wasRefreshed: true
                );

                QueueRequestAccessTokenWithRefreshToken();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAccessTokenWithRefreshTokenCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnRequestAvailableDevicesCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseDevices>(
					json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
				);
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnRequestCurrentTrackCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseCurrentTrack>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                var track = spotifyResponse.Track;
                if (track is not null)
                {
                    SaveArtistAndTrackInTwitchData(
                        track: track
                    );

                    CurrentTrackRetrieved?.Invoke(
                        obj: m_spotifyTwitchData    
                    );

                    ResetSpotifyTwitchData();
                    return;
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }

            m_spotifyTwitchData.Message =
                $"could not retrieve current playing song.";
            m_spotifyTwitchData.OnScreenMessage =
                $"could not retrieve current playing song" +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

            Errored?.Invoke(
                obj: m_spotifyTwitchData
            );

            ResetSpotifyTwitchData();
        }

        private void OnRequestCurrentTrackBeforeSkipCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseCurrentTrack>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                var track = spotifyResponse.Track;
                if (track is not null)
                {
                    SaveArtistAndTrackInTwitchData(
                        track: track
                    );
                    RequestSkipToNext();
                    return;
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestAvailableDevicesCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }

            m_spotifyTwitchData.Message =
                $"could not retrieve current playing song. Failed to skip song.";
            m_spotifyTwitchData.OnScreenMessage =
                $"could not retrieve current playing song" +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

            Errored?.Invoke(
                obj: m_spotifyTwitchData
            );

            ResetSpotifyTwitchData();
        }

        private void OnRequestPlaybackStateCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestPlaybackStateCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponsePlaybackState>(
					json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
				);
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestPlaybackStateCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnRequestSkipToNextCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestSkipToNextCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                TrackSkipped?.Invoke(
                    obj: m_spotifyTwitchData
                );
                ResetSpotifyTwitchData();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestSkipToNextCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif

                m_spotifyTwitchData.Message = 
                    $"current track failed to skip.";
                m_spotifyTwitchData.OnScreenMessage = 
                    $"current track failed to skip" +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

                Errored?.Invoke(
                    obj: m_spotifyTwitchData
                );

                ResetSpotifyTwitchData();
            }
        }

        private void OnRequestTrackQueueCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestTrackQueueCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                RequestUserQueueAfterTrackQueued();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestTrackQueueCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
                m_spotifyTwitchData.Message =
                    $"failed to queue track " +
                    $"\"" +
                    $"{m_spotifyTwitchData.TrackName} " +
                    $"by " +
                    $"{m_spotifyTwitchData.ArtistName}" +
                    $".\"";
                m_spotifyTwitchData.OnScreenMessage =
                    $"failed to queue the track " +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: "\"")}" +
                    $"{TwitchChatColorCodes.ConvertToSubErrorMessage(message: m_spotifyTwitchData.TrackName)} " +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: "\" by \"")}" +
                    $"{TwitchChatColorCodes.ConvertToSubErrorMessage(message: m_spotifyTwitchData.ArtistName)}" +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".\"")}";

                Errored?.Invoke(
                    obj: m_spotifyTwitchData
                );

                ResetSpotifyTwitchData();
            }
        }

        private void OnRequestTrackSearchCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestTrackSearchCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseSearchItem>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                if (spotifyResponse is not null)
                {
                    var tracks = spotifyResponse.Tracks;
                    if (tracks is not null)
                    {
                        var items = tracks.Items;
                        if (items is not null && items.Length > 0)
                        {
                            var track = items[0];
                            var uri = track.Uri;
                            RequestTrackAddedToQueue(
                                trackUri: uri
                            );
                            return;
                        }
                    }
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestTrackSearchCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }

            m_spotifyTwitchData.Message =
                $"could not find a track using " +
                $"\"" +
                $"{m_spotifyTwitchData.SearchParameters}" +
                $".\"";
            m_spotifyTwitchData.OnScreenMessage =
                $"could not find a track using " +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: "\"")}" +
                $"{TwitchChatColorCodes.ConvertToSuccessMessage(message: m_spotifyTwitchData.SearchParameters)}" +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".\"")}";

            Errored?.Invoke(
                obj: m_spotifyTwitchData
            );

            ResetSpotifyTwitchData();
        }

        private void OnRequestUserAuthorizationCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserAuthorizationCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseAccessToken>(
					json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
				);
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserAuthorizationCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnRequestUserQueueAfterTrackQueuedCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueAfterTrackQueuedCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseQueue>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                var queue = spotifyResponse.Queue;
                for (var i = 0; i < queue.Length; i++)
                {
                    var newQueueTrack = queue[i];
                    var oldQueueTrack = m_spotifyTrackQueue[i];

                    if (
                        newQueueTrack.Id.Equals(
                            value: oldQueueTrack.Id
                        ) is false
                    )
                    {
                        m_spotifyTwitchData.QueuePosition = i + 1;
                        m_spotifyTwitchData.TrackId = newQueueTrack.Id;
                        SaveArtistAndTrackInTwitchData(
                            track: newQueueTrack
                        );

                        m_currentSpotifyQueuedUserTrackDatas.Enqueue(
                            item: new()
                            {
                                ArtistName = m_spotifyTwitchData.ArtistName,
                                TrackId = m_spotifyTwitchData.TrackId,
                                TrackName = m_spotifyTwitchData.TrackName,
                                TwitchUserName = m_spotifyTwitchData.TwitchUserName,
                            }
                        );

                        TrackQueuedCompleted?.Invoke(
                            obj: m_spotifyTwitchData
                        );
                        ResetSpotifyTwitchData();
                        return;
                    }
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueAfterTrackQueuedCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }

            m_spotifyTwitchData.Message = 
                $"queue failed to retrieve after queuing track.";
            m_spotifyTwitchData.OnScreenMessage = 
                $"queue failed to retrieve after queuing track" +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

            Errored?.Invoke(
                obj: m_spotifyTwitchData
            );
            ResetSpotifyTwitchData();
        }

        private void OnRequestUserQueueBeforeTrackQueuedBySearchTermsCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueBeforeTrackQueuedBySearchTermsCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseQueue>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                m_spotifyTrackQueue = spotifyResponse.Queue;
                RequestTrackQueueBySearchTerms();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueBeforeTrackQueuedBySearchTermsCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif

                m_spotifyTwitchData.Message = 
                    $"queue failed to retrieve after queuing track.";
                m_spotifyTwitchData.OnScreenMessage = 
                    $"queue failed to retrieve after queuing track" +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

                Errored?.Invoke(
                    obj: m_spotifyTwitchData
                );
                ResetSpotifyTwitchData();
            }
        }

        private void OnRequestUserQueueBeforeTrackQueuedByTrackIdCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueBeforeTrackQueuedByTrackIdCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseQueue>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                m_spotifyTrackQueue = spotifyResponse.Queue;
                RequestTrackQueueByTrackId();
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueBeforeTrackQueuedByTrackIdCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif

                m_spotifyTwitchData.Message = 
                    $"queue failed to retrieve after queuing track.";
                m_spotifyTwitchData.OnScreenMessage = 
                    $"queue failed to retrieve after queuing track" +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

                Errored?.Invoke(
                    obj: m_spotifyTwitchData
                );
                ResetSpotifyTwitchData();
            }
        }

        private void OnRequestUserQueueForUserCompleted(
            long result,
            long responseCode,
            string[] headers,
            byte[] body
        )
        {
            if (
                HttpManager.IsResponseCodeSuccessful(
                    responseCode: responseCode
                ) is true
            )
            {
#if DEBUG
                GD.Print(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueForUserCompleted)}() - Web request {responseCode} POST successful."
                );
#endif

                var spotifyResponse = JsonSerializer.Deserialize<SpotifyResponseQueue>(
                    json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
                );

                var queue = spotifyResponse.Queue;
                for (var i = 0; i < queue.Length; i++)
                {
                    var newQueueTrack = queue[i];
                    var spotifyQueueTrackId = newQueueTrack.Id;

                    var frontUserTrackData = m_currentSpotifyQueuedUserTrackDatas.Peek();
                    var localQueueTrackId = frontUserTrackData.TrackId;

                    if (
                        localQueueTrackId.Equals(
                            value: spotifyQueueTrackId
                        ) is true
                    )
                    {
                        m_spotifyTwitchData.SpotifyQueuedUserTracks = m_currentSpotifyQueuedUserTrackDatas;
                        UserTrackQueueRetrieveCompleted?.Invoke(
                            obj: m_spotifyTwitchData
                        );
                        ResetSpotifyTwitchData();
                        return;
                    }

                    _ = m_currentSpotifyQueuedUserTrackDatas.Dequeue();
                    if (m_currentSpotifyQueuedUserTrackDatas.Count is 0)
                    {
                        m_spotifyTwitchData.Message = 
                            $"there are no song requests currently queued for viewers.";
                        m_spotifyTwitchData.OnScreenMessage = 
                            $"there are no song requests currently queued for viewers" +
                            $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";
                        UserTrackQueueRetrieveFailed?.Invoke(
                            obj: m_spotifyTwitchData    
                        );
                        ResetSpotifyTwitchData();
                        return;
                    }

                    i--;
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestUserQueueForUserCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }

            m_spotifyTwitchData.Message = 
                $"user queue failed to retrieve.";
            m_spotifyTwitchData.OnScreenMessage = 
                $"user queue failed to retrieve" +
                $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

            Errored?.Invoke(
                obj: m_spotifyTwitchData
            );
          
            ResetSpotifyTwitchData();
        }

        public static string ParseSearchTextForTrackId(
            string searchText    
        )
        {
            var searchTextSplit = searchText.Split(
                separator: '?'
            );

            foreach (var subSearchText in searchTextSplit)
            {
                if (
                    subSearchText.Contains(
                        value: c_uriSpotifyTrackSecure
                    ) is true
                )
                {
                    var subSearchTextSplit = subSearchText.Split(
                        divisor: c_uriSpotifyTrackSecure
                    );
                    return subSearchTextSplit.Last();
                }
                else if (
                    subSearchText.Contains(
                        value: c_uriSpotifyTrackInsecure
                    ) is true
                )
                {
                    var subSearchTextSplit = subSearchText.Split(
                        divisor: c_uriSpotifyTrackInsecure
                    );
                    return subSearchTextSplit.Last();
                }
            }

            return string.Empty;
        }

        private void ProcessHttpRequests()
        {
            if (m_spotifyTwitchData is null)
            {
                lock (m_currentSpotifyTwitchDatasLock)
                {
                    if (m_currentSpotifyTwitchDatas.Count > 0u)
                    {
                        m_spotifyTwitchData = m_currentSpotifyTwitchDatas.Dequeue();
                    }
                    else
                    {
                        return;
                    }
                }

                var spotifyTwitchDataRequestType = m_spotifyTwitchData.SpotifyTwitchDataRequestType;
                switch (spotifyTwitchDataRequestType)
                {
                    case SpotifyTwitchDataRequestType.CurrentTrack:
                        RequestCurrentTrack();
                        break;

                    case SpotifyTwitchDataRequestType.TrackQueueBySearchTerms:
                        RequestUserQueueBeforeTrackQueuedBySearchTerms();
                        break;

                    case SpotifyTwitchDataRequestType.TrackQueueByTrackId:
                        RequestUserQueueBeforeTrackQueuedByTrackId();
                        break;

                    case SpotifyTwitchDataRequestType.TrackSkip:
                        RequestCurrentTrackAndSkip();
                        break;

                    case SpotifyTwitchDataRequestType.UserTrackQueue:
                        RequestUserQueueForUser();
                        break;

                    default:
                        break;
                }
            }
        }

        private void QueueRequestAccessTokenWithRefreshToken()
        {
            _ = Task.Run(
                function:
                async () =>
                {
                    var expireTime = DateTime.Parse(
                        s: m_spotifyAccessToken.ExpireTime
                    );
                    var remainingTime = expireTime - DateTime.UtcNow;

                    await Task.Delay(
                        millisecondsDelay: (int)remainingTime.TotalMilliseconds
                    );

                    RequestAccessTokenWithRefreshToken();
                }
            );
        }

        private void RequestAccessToken()
        {
            var headers = new List<string>()
            {
                $"Content-Type: application/x-www-form-urlencoded",
                $"Authorization: Basic {Convert.ToBase64String(inArray: Encoding.UTF8.GetBytes(s: $"{m_spotifyData.ClientId}:{m_spotifyData.ClientSecret}"))}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAccessToken}",
                headers: headers,
                method: Method.Post,
                json:
                    $"grant_type=authorization_code&" +
                    $"code={c_authorizationCode}&" +
                    $"redirect_uri={c_redirectUri}",
                requestCompletedHandler: OnRequestAccessTokenCompleted
            );
        }

        private void RequestAccessTokenWithRefreshToken()
        {
            var headers = new List<string>()
            {
                $"Content-Type: application/x-www-form-urlencoded",
                $"Authorization: Basic {Convert.ToBase64String(inArray: Encoding.UTF8.GetBytes(s: $"{m_spotifyData.ClientId}:{m_spotifyData.ClientSecret}"))}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAccessToken}",
                headers: headers,
                method: Method.Post,
                json:
                    $"grant_type=refresh_token&" +
                    $"refresh_token={m_spotifyAccessToken.RefreshToken}",
                requestCompletedHandler: OnRequestAccessTokenWithRefreshTokenCompleted
            );
        }

        private void RequestCurrentTrack()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/currently-playing",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestCurrentTrackCompleted
            );
        }

        private void RequestCurrentTrackAndSkip()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/currently-playing",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestCurrentTrackBeforeSkipCompleted
            );    
        }

        private void RequestSkipToNext()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/next",
                headers: headers,
                method: Method.Post,
                json: string.Empty,
                requestCompletedHandler: OnRequestSkipToNextCompleted
            );
        }

        private void RequestTrackAddedToQueue(
            string trackUri
        )
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue?uri={Uri.EscapeDataString(stringToEscape: trackUri)}",
                headers: headers,
                method: Method.Post,
                json: string.Empty,
                requestCompletedHandler: OnRequestTrackQueueCompleted
            );
        }

        private void RequestTrackQueueBySearchTerms()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/search?q={Uri.EscapeDataString(stringToEscape: m_spotifyTwitchData.SearchParameters)}&type=track&limit=1",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestTrackSearchCompleted
            );
        }

        private void RequestTrackQueueByTrackId()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue?uri={Uri.EscapeDataString(stringToEscape: $"spotify:track:{m_spotifyTwitchData.TrackId}")}",
                headers: headers,
                method: Method.Post,
                json: string.Empty,
                requestCompletedHandler: OnRequestTrackQueueCompleted
            );
        }

        private void RequestUserAuthorization()
        {
            _ = OS.ShellOpen(
                uri: $"https://accounts.spotify.com/authorize?" +
                     $"client_id={m_spotifyData.ClientId}&" +
                     $"response_type=code&" +
                     $"redirect_uri={c_redirectUri}&" +
                     $"scope={Uri.EscapeDataString(c_userAccessScopes)}"
            );
        }

        private void RequestUserQueueAfterTrackQueued()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestUserQueueAfterTrackQueuedCompleted
            );
        }

        private void RequestUserQueueBeforeTrackQueuedBySearchTerms()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestUserQueueBeforeTrackQueuedBySearchTermsCompleted
            );
        }

        private void RequestUserQueueBeforeTrackQueuedByTrackId()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestUserQueueBeforeTrackQueuedByTrackIdCompleted
            );
        }

        private void RequestUserQueueForUser()
        {
            if (m_currentSpotifyQueuedUserTrackDatas.Count is 0)
            {
                m_spotifyTwitchData.Message = 
                    $"there are no song requests currently queued for viewers.";
                m_spotifyTwitchData.OnScreenMessage = $"" +
                    $"there are no song requests currently queued for viewers" +
                    $"{TwitchChatColorCodes.ConvertToNormalMessage(message: ".")}";

                UserTrackQueueRetrieveFailed?.Invoke(
                    obj: m_spotifyTwitchData
                );

                ResetSpotifyTwitchData();
                return;
            }

            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_uriAPI}/me/player/queue",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestUserQueueForUserCompleted
            );
        }

        private void ResetSpotifyTwitchData()
        {
            m_spotifyTwitchData = null;
        }

        private void RetrieveResources()
        {
            var spotifyAccessTokenBody = ApplicationManager.ReadRequiredFile(
				requiredFileType: RequiredFileType.SpotifyAccessToken
            );
            m_spotifyAccessToken = JsonSerializer.Deserialize<SpotifyAccessToken>(
                json: Encoding.UTF8.GetString(
                    bytes: spotifyAccessTokenBody,
                    index: 0,
                    count: spotifyAccessTokenBody.Length
                )
            );

            var spotifyDataBody = ApplicationManager.ReadRequiredFile(
				requiredFileType: RequiredFileType.SpotifyData
            );
            m_spotifyData = JsonSerializer.Deserialize<SpotifyData>(
                json: Encoding.UTF8.GetString(
                    bytes: spotifyDataBody,
                    index: 0,
                    count: spotifyDataBody.Length
                )
            );

            m_httpManager = GetNode<HttpManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.HttpManager
                )
            );

            if (
                IsAccessTokenExpired() is true
            )
            {
                RequestAccessTokenWithRefreshToken();
            }
            else
            {
                QueueRequestAccessTokenWithRefreshToken();
            }

            BindTwitchChannelPointRewards();
        }

        private void SaveArtistAndTrackInTwitchData(
            SpotifyResponseTrack track    
        )
        {
            var artists = track.Artists;
            var names = artists.Select(
                selector: a => 
                a.Name
            ).ToArray();

            m_spotifyTwitchData.ArtistName =
                names.Length is 1
                ? names[0]
                : $"{string.Join(separator: ", ", values: names.Take(count: names.Length - 1))}" +
                  $"{(names.Length is 2 ? string.Empty : ",")} " +
                  $"& {names.Last()}";
            m_spotifyTwitchData.TrackName = track.Name;
        }

        private void WriteAccessToken(
            SpotifyResponseAccessToken response,
            bool wasRefreshed
        )
        {
            m_spotifyAccessToken.AccessToken = response.AccessToken;
            if (wasRefreshed is false)
            {
                m_spotifyAccessToken.RefreshToken = response.RefreshToken;
            }
            m_spotifyAccessToken.ExpireTime = $"{DateTime.UtcNow.AddHours(value: 1):yyyy-MM-dd HH:mm:ss}";

            ApplicationManager.WriteRequiredFile(
                requiredFileType: RequiredFileType.SpotifyAccessToken,
                bytes: Encoding.UTF8.GetBytes(
                    s: JsonSerializer.Serialize(
                        value: m_spotifyAccessToken
                    )
                )
            );
        }
    }
}