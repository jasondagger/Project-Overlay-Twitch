
namespace Overlay
{
    using Godot;
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using NodeType = NodeDirectory.NodeType;
    using RequiredFileType = ApplicationManager.RequiredFileType;
    using HttpClient = System.Net.Http.HttpClient;
    using static Godot.HttpClient;
    using System.Collections.Generic;

    public sealed partial class SpotifyManager : Node
    {
        #region GODOT_INTRINSTICS
        public override void _Ready()
        {
            RetrieveResources();
        }
        #endregion

        public void AttemptToQueueTrackWithSearchParameters(
            string searchParameters
        )
        {
            RequestTrackUriBySearchParameters(
                searchParameters: searchParameters    
            );
        }

        #region INTERNAL_VARIABLES_&_STRUCTURES
        private const int c_accessTokenRefreshTimeInMilliseconds = 3600000;
        private const string c_authorizationCode = "AQCsZ-HvlDJyhOOJ52TZsvnNZQAibiQXupDEjdTrK497FW8oTXj1q8P3vLGm89yP8JIfZkIR_jO3s7Rn63RPRW6xC9ocRWPPal4xqEaBLiglYZBIS3TL0XJ84rI-2axiODaFH27kLnLGyd38hBBzE9aieNBdCi0_B5B1i-iizw4eswPVp84qWyarLLea9u6N4fj3QZUs_9TJ6od-zflrYl34oy4Q0iEpvdy9YbJwd2vsNxf8shpcE2fsHD5zA4wGw60znkZRkGAtqzkg5ZFbaOVCIXco";
        private const string c_redirectUri = "http://localhost:8888/callback";
        private const string c_urlAPI = "https://api.spotify.com/v1";
        private const string c_urlAccessToken = "https://accounts.spotify.com/api/token";
        private const string c_userAccessScopes = "user-modify-playback-state user-read-currently-playing user-read-playback-state";

        private readonly HttpClient httpClient = new();

        private HttpManager m_httpManager = null;
        private SpotifyAccessToken m_spotifyAccessToken = null;
        private SpotifyData m_spotifyData = null;
        #endregion

        #region INTERNAL_FLAGS
        private bool IsAccessTokenExpired()
        {
            return DateTime.Compare(
                t1: DateTime.Parse(
                    s: $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                ),
                t2: DateTime.Parse(
                    s: m_spotifyAccessToken.ExpireTime
                )
            ) >= 0;
        }
        #endregion

        #region INTERNAL_EVENT_HANDLERS
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
                QueueAccessTokenRefresh();
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

                _ = Task.Run(
                    function: 
                    async () =>
                    {
                        await Task.Delay(
                            millisecondsDelay: c_accessTokenRefreshTimeInMilliseconds
                        );

                        RequestAccessToken();
                    }
                );
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
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnRequestSkipToNextCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
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

        private void OnTrackQueueCompleted(
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
                    what: $"{nameof(SpotifyManager)}.{nameof(OnTrackQueueCompleted)}() - Web request {responseCode} POST successful."
                );
#endif
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnTrackQueueCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }

        private void OnTrackSearchCompleted(
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
                    what: $"{nameof(SpotifyManager)}.{nameof(OnTrackSearchCompleted)}() - Web request {responseCode} POST successful."
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
                            RequestTrackAddedToQueue(
                                trackUri: items[0].Uri    
                            );
                        }
                    }
                }
            }
            else
            {
#if DEBUG
                GD.PrintErr(
                    what: $"{nameof(SpotifyManager)}.{nameof(OnTrackSearchCompleted)}() - Web request POST failed with code {responseCode}."
                );
#endif
            }
        }
        #endregion

        private void QueueAccessTokenRefresh()
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
                        millisecondsDelay: (int)remainingTime.TotalMilliseconds - 20000
                    );

                    RequestAccessToken();
                }
            );
        }

        #region INTERNAL_HTTP_REQUESTS
        private void RequestAccessToken()
        {
            var headers = new List<string>()
            {
                $"Content-Type: application/x-www-form-urlencoded",
                $"Authorization: Basic {Convert.ToBase64String(inArray: Encoding.UTF8.GetBytes(s: $"{m_spotifyData.ClientId}:{m_spotifyData.ClientSecret}"))}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_urlAccessToken}",
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
                url: $"{c_urlAccessToken}",
                headers: headers,
                method: Method.Post,
                json:
                    $"grant_type=refresh_token&" +
                    $"refresh_token={m_spotifyAccessToken.RefreshToken}",
                requestCompletedHandler: OnRequestAccessTokenWithRefreshTokenCompleted
            );
        }

        private void RequestAvailableDevices()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/me/player/devices",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestAvailableDevicesCompleted
            );
        }

        private void RequestPlaybackState()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/me/player",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestPlaybackStateCompleted
            );
        }

        private void RequestSkipToNext()
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/me/player/next",
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
                url: $"{c_urlAPI}/me/player/queue?uri={Uri.EscapeDataString(stringToEscape: trackUri)}",
                headers: headers,
                method: Method.Post,
                json: string.Empty,
                requestCompletedHandler: OnTrackQueueCompleted
            );
        }

        private void RequestTrackUriBySearchParameters(
            string searchParameters    
        )
        {
            var headers = new List<string>()
            {
                $"Authorization: Bearer {m_spotifyAccessToken.AccessToken}",
            };
            m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/search?q={Uri.EscapeDataString(stringToEscape: searchParameters)}&type=track&limit=1",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnTrackSearchCompleted
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
        #endregion

        #region INTERNAL_INITIALIZATION
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
                path: NodeDirectory.NodePaths[NodeType.HttpManager]
            );

            //RequestUserAuthorization();
            //RequestAccessToken();

            if (
                IsAccessTokenExpired() is true
            )
            {
                RequestAccessTokenWithRefreshToken();
            }
            else
            {
                QueueAccessTokenRefresh();
            }
        }
        #endregion

        #region INTERNAL_FILE_WRITE
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
        #endregion
    }
}