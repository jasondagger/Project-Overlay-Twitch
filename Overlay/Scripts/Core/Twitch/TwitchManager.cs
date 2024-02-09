using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpClient;
using ChannelPointReward = TwitchChannelPointRewardsManager.ChannelPointReward;
using ChannelPointRewardType = TwitchChannelPointRewardsManager.ChannelPointRewardsType;
using NodeType = NodeDirectory.NodeType;
using PlaylistType = AudioManager.PlaylistType;

public sealed partial class TwitchManager : Node
{
    public Action<
        TwitchWebSocketMessagePayloadEventChannelChatNotification
    > ChannelChatNotification = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelCheer
    > ChannelCheered = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelFollow
    > ChannelFollowed = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed
    > ChannelPointsCustomRewardRedeemed = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelRaid
    > ChannelRaided = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelSubscribe
    > ChannelSubscribed = null;
    public Action<
        TwitchWebSocketMessagePayloadEventChannelSubscriptionGift
    > ChannelSubscriptionGifted = null;

    public Action<
        TwitchResponseChannelFollowersData[]
    > FollowersRetrieved = null;
    public Action<
        TwitchResponseUsersSubscribersData[]
    > GiftedSubscribersRetrieved = null;
    public Action<
        TwitchResponseUsersSubscribersData[]
    > SubscribersRetrieved = null;

    public override void _EnterTree()
    {
        // retrieve user access token
        //OS.ShellOpen(
        //    $"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=vf0zlx9k3mnijlxyychhuw3z5ls8km&redirect_uri=http://localhost:3000&scope=bits%3Aread%20channel%3Aread%3Asubscriptions%20channel%3Amanage%3Aredemptions%20moderator%3Aread%3Afollowers%20user%3Aread%3Achat"
        //);

        RetrieveResources();
    }

    public override void _ExitTree()
    {
        m_shutdown = true;
    }

    public override void _Notification(
        int what
    )
    {
        switch (what)
        {
            case (int)NotificationWMCloseRequest:
                m_shutdown = true;
                RequestChannelPointRewardPatchRedeemCanceled();
                break;
        }
    }

    public override void _Process(
        double delta
    )
    {
        if (m_messageQueue.Count > 0u)
        {
            var message = m_messageQueue.Dequeue();
            var type = message.type;

            switch (type)
            {
                case TwitchEventSubSubscriptionType.ChannelChatNotification:
                    var messageChannelNotification = message as TwitchMessageChannelChatNotification;
                    ChannelChatNotification?.Invoke(
                        messageChannelNotification.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelCheer:
                    var messageChannelCheer = message as TwitchMessageChannelCheer;
                    ChannelCheered?.Invoke(
                        messageChannelCheer.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelFollow:
                    var messageChannelFollow = message as TwitchMessageChannelFollow;
                    ChannelFollowed?.Invoke(
                        messageChannelFollow.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed:
                    var messageChannelPointsCustomRewardRedeemed = message as TwitchMessageChannelPointsCustomRewardRedeemed;
                    ChannelPointsCustomRewardRedeemed?.Invoke(
                        messageChannelPointsCustomRewardRedeemed.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelRaid:
                    var messageChannelRaid = message as TwitchMessageChannelRaid;
                    ChannelRaided?.Invoke(
                        messageChannelRaid.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelSubscribe:
                    var messageChannelSubscribe = message as TwitchMessageChannelSubscribe;
                    ChannelSubscribed?.Invoke(
                         messageChannelSubscribe.@event
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelSubscriptionGift:
                    var messageChannelSubscriptionGift = message as TwitchMessageChannelSubscriptionGift;
                    ChannelSubscriptionGifted?.Invoke(
                         messageChannelSubscriptionGift.@event
                    );
                    break;

                default:
                    break;
            }
        }
    }

    public void ClaimCustomChannelPointReward(
        ChannelPointRewardType channelPointRewardsType,
        string id
    )
    {
        switch (channelPointRewardsType)
        {
            case ChannelPointRewardType.CommandSetPlaylist:
                RequestChannelPointRewardPatchRedeemSetPlaylist(
                    id,
                    true
                );
                break;

            default:
                break;
        }
    }

    public override void _Ready()
    {
        ConnectWebSocket();

        RequestChannelPointRewardAdd();
        RequestChannelPointRewardUpdateSetPlaylist();
        RequestFollowers(
            string.Empty
        );
        RequestSubscribers(
            string.Empty
        );
    }

    public void RefundCustomChannelPointReward(
        ChannelPointRewardType channelPointRewardsType,
        string id
    )
    {
        switch (channelPointRewardsType)
        {
            case ChannelPointRewardType.CommandSetPlaylist:
                RequestChannelPointRewardPatchRedeemSetPlaylist(
                    id,
                    false
                );
                break;

            default:
                break;
        }
    }

    public List<TwitchResponseChannelFollowersData> GetChannelFollowers()
    {
        return m_channelFollowers;
    }

    public List<TwitchResponseUsersSubscribersData> GetChannelSubscribers()
    {
        return m_channelSubscribers;
    }

    private const string c_urlAPI = "https://api.twitch.tv/helix";
    private const string c_urlOAuth = "https://id.twitch.tv/oauth2/token";
    private const string c_webSocketAddress = "wss://eventsub.wss.twitch.tv/ws";

    private const char c_twitchUTCSuffix = 'S';

    private readonly Dictionary<ChannelPointRewardType, string> m_channelPointRewardIds = new()
    {
        { ChannelPointRewardType.CommandRequestSong,     "219af672-853c-438f-8d09-54b7fad7af49" },
        { ChannelPointRewardType.CommandSetPlaylist,     "ab04cb34-6b69-4082-b632-06b18453d412" },
        { ChannelPointRewardType.IRLHydrate,             "583a4ba8-ed2d-45c7-820a-588a0c2e8a15" },
        { ChannelPointRewardType.IRLNoCursing,           "41628d62-a144-4bad-98be-99f0dabcdd02" },
        { ChannelPointRewardType.IRLPostureCheck,        "21bf998e-41e3-45ec-9d28-f54c3de85f41" },
        { ChannelPointRewardType.IRLShowKitty,           "fd3f5dc7-7c13-48eb-8fc6-c20c189d7788" },
        { ChannelPointRewardType.IRLShowPuppy,           "9fdb73b2-ad6d-489f-be2e-ee87932918d4" },
        { ChannelPointRewardType.IRLStreeeeeetch,        "938ea7ec-506e-4dbb-8122-59f913069ba3" },
        { ChannelPointRewardType.SoundAlertApplause,     "265e1208-03db-41ec-96b9-9606b02272aa" },
        { ChannelPointRewardType.SoundAlertFirstBlood,   "ec7e240c-ce14-4a9f-abaa-d5a70733ac4a" },
        { ChannelPointRewardType.SoundAlertGodlike,      "7afe008e-9f04-4b6d-a1ac-a2d605387789" },
        { ChannelPointRewardType.SoundAlertHeartbeat,    "40931e72-0530-43e1-a4e3-565c053c41a7" },
        { ChannelPointRewardType.SoundAlertHolyShit,     "41ddd4ce-a65a-4089-88e5-0ae3ab75f20b" },
        { ChannelPointRewardType.SoundAlertHowdy,        "a0586a0f-a24f-4d43-8e5a-7e79b267a9ff" },
        { ChannelPointRewardType.SoundAlertKegExplosion, "eead1d30-b76c-40f9-9454-7872944d4281" },
        { ChannelPointRewardType.SoundAlertKegFuse,      "94e3188e-51af-4377-9815-cb8b385f669e" },
        { ChannelPointRewardType.SoundAlertNice,         "162c662b-e007-49d0-b91c-0fccd8ba9f3b" },
    };

    private struct SubscriptionGift
    {
        public string user_id;
        public int total;

        public SubscriptionGift(
            string user_id,
            int total
        )
        {
            this.user_id = user_id;
            this.total = total;
        }
    }

    private AudioManager m_audioManager = null;
    private HttpManager m_httpManager = null;
    private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
    private ClientWebSocket m_webSocket = new();

    private Queue<TwitchMessage> m_messageQueue = new();
    private bool m_shutdown = false;

    private List<TwitchResponseUsersSubscribersData> m_channelSubscribers = new();
    private List<TwitchResponseChannelFollowersData> m_channelFollowers = new();
    private List<TwitchResponseUsersSubscribersData> m_giftedSubscribers = new();

    private Queue<SubscriptionGift> m_pendingSubscriptionGifts = new();

    private void RetrieveResources()
    {
        m_audioManager = GetNode<AudioManager>(
            NodeDirectory.NodePaths[NodeType.AudioManager]
        );
        m_httpManager = GetNode<HttpManager>(
            NodeDirectory.NodePaths[NodeType.HttpManager]
        );
        m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
            NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
        );

        m_audioManager.ChangedPlaylist += OnAudioManagerPlaylistChanged;
    }

    private async void ConnectWebSocket()
    {
        await Task.Run(
            async () =>
            {
                // connect to Twitch websocket
                Uri uri = new(
                    c_webSocketAddress
                );

                await m_webSocket.ConnectAsync(
                    uri,
                    default
                );

#if DEBUG
                GD.Print(
                    $"{nameof(TwitchManager)}.{nameof(ConnectWebSocket)}() - Web socket connect successful."
                );
#endif

                while (!m_shutdown)
                {
                    if (m_webSocket.State == WebSocketState.Open)
                    {
                        var bytes = new byte[4096u];
                        var result = await m_webSocket.ReceiveAsync(
                            bytes,
                            default
                        );

                        // https://dev.twitch.tv/docs/eventsub/handling-websocket-events/
                        var webSocketMessage = ParseWebSocketMessage(
                            bytes,
                            result
                        );
                        ReadWebSocketMessage(
                            webSocketMessage
                        );
                    }
                }

                RetrieveEventSubSubscriptions();
            }
        );
    }

    private void DeleteEventSubSubscriptions(
        string json
    )
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };

        // delete all existing event subscriptions
        var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseEventSubSubscriptions>(
            json
        );
        foreach (var data in twitchResponse.data)
        {
            m_httpManager.SendHttpRequest(
                $"{c_urlAPI}/eventsub/subscriptions?id={data.id}",
                headers,
                Method.Delete,
                string.Empty,
                OnDeletedEventSubSubscription
            );
        }
    }

    private void HandleWebSocketMessageNotification(
        TwitchWebSocketMessage message
    )
    {
        switch (message.metadata.subscription_type)
        {
            case "channel.chat.notification":
                var messageChannelChatNotification = message as TwitchWebSocketMessageChannelChatNotification;
                var payloadChannelChatNotification = messageChannelChatNotification.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelChatNotification(
                        payloadChannelChatNotification.@event
                    )
                );
                break;

            case "channel.cheer":
                var messageChannelCheer = message as TwitchWebSocketMessageChannelCheer;
                var payloadChannelCheer = messageChannelCheer.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelCheer(
                        payloadChannelCheer.@event
                    )
                );
                break;

            case "channel.follow":
                var messageChannelFollow = message as TwitchWebSocketMessageChannelFollow;
                var payloadChannelFollow = messageChannelFollow.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelFollow(
                        payloadChannelFollow.@event
                    )
                );
                RequestLatestFollower();
                break;

            case "channel.channel_points_custom_reward_redemption.add":
                var messageChannelPointsRedeemed = message as TwitchWebSocketMessageChannelPointsCustomRewardRedeemed;
                var payloadChannelPointsRedeemed = messageChannelPointsRedeemed.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelPointsCustomRewardRedeemed(
                        payloadChannelPointsRedeemed.@event
                    )
                );
                break;

            case "channel.raid":
                var messageChannelRaid = message as TwitchWebSocketMessageChannelRaid;
                var payloadChannelRaid = messageChannelRaid.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelRaid(
                        payloadChannelRaid.@event
                    )
                );
                break;

            case "channel.subscribe":
                var messageChannelSubscribe = message as TwitchWebSocketMessageChannelSubscribe;
                var payloadChannelSubscribe = messageChannelSubscribe.payload;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelSubscribe(
                        payloadChannelSubscribe.@event
                    )
                );
                RequestSubscribers(
                    string.Empty
                );
                break;

            case "channel.subscription.gift":
                var messageChannelSubscriptionGift = message as TwitchWebSocketMessageChannelSubscriptionGift;
                var payloadChannelSubscriptionGift = messageChannelSubscriptionGift.payload;
                var eventChannelSubscriptionGift = payloadChannelSubscriptionGift.@event;
                m_messageQueue.Enqueue(
                    new TwitchMessageChannelSubscriptionGift(
                        eventChannelSubscriptionGift
                    )
                );

                m_pendingSubscriptionGifts.Enqueue(
                    new(
                        eventChannelSubscriptionGift.user_id,
                        eventChannelSubscriptionGift.total
                    )
                );
                if (m_pendingSubscriptionGifts.Count == 1u)
                {
                    RequestGiftedSubscribers(
                        string.Empty
                    );
                }

                RequestSubscribers(
                    string.Empty
                );
                break;

            default:
                return;
        }
    }

    private void HandleWebSocketMessageRevocation(
        TwitchWebSocketMessage message
    )
    {
        // todo: ???

    }

    private void HandleWebSocketMessageSessionKeepAlive(
        TwitchWebSocketMessage message
    )
    {
        // todo: ???

    }

    private void HandleWebSocketMessageSessionReconnect(
        TwitchWebSocketMessage message
    )
    {
        // todo: ???
    }

    private void HandleWebSocketMessageSessionWelcome(
        TwitchWebSocketMessage message
    )
    {
        RegisterEventSubSubscriptions(
            message.payload.session.id
        );
    }

    private bool WasHttpResponseSuccessful(
        long responseCode    
    )
    {
        return responseCode >= 200u && responseCode < 300u;
    }

    private void OnAudioManagerPlaylistChanged()
    {
        Task.Run(
            RequestChannelPointRewardUpdateSetPlaylist
        );
    }

    private void OnDeletedEventSubSubscription(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(DeleteEventSubSubscriptions)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(DeleteEventSubSubscriptions)}() - Web request POST failed with code {responseCode}."
            );
        }
#endif
    }

    private void OnRegisteredEventSubSubscription(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RegisterEventSubSubscriptions)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RegisterEventSubSubscriptions)}() - Web request POST failed with code {responseCode}."
            );
        }
#endif
    }

    private void OnRequestChannelPointRewardAdded(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardAdd)}() - Web request {responseCode} POST successful."
            );
#endif

            var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseCustomRewardCreate>(
                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );

            var channelRewardType = m_twitchChannelPointRewardsManager.GetRewardTypeByName(
                twitchResponse.data[0].title
            );
            m_channelPointRewardIds[channelRewardType] = twitchResponse.data[0].id;

#if DEBUG
            GD.Print(
                $"{channelRewardType} Id: {m_channelPointRewardIds[channelRewardType]}"
            );
#endif
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardAdd)}() - Web request POST failed with {responseCode}."
            );
#endif
        }
    }

    private void OnRequestChannelPointRewardDeleted(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardDelete)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardDelete)}() - Web request POST failed with {responseCode}."
            );
        }
#endif
    }

    private void OnRequestChannelPointRewardRedeemedSetPlaylist(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemSetPlaylist)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemSetPlaylist)}() - Web request POST failed with {responseCode}."
            );
        }
#endif
    }

    private void OnRequestChannelPointRewardRedeemCanceled(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemSetPlaylist)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemSetPlaylist)}() - Web request POST failed with {responseCode}."
            );
        }
#endif
    }

    private void OnRequestChannelPointRewardUpdatedSetPlaylist(
        long result,
        long responseCode,
        string[] headers,
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardUpdateSetPlaylist)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardUpdateSetPlaylist)}() - Web request POST failed with {responseCode}."
            );
        }
#endif
    }

    private void OnRequestFollowersCompleted(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request {responseCode} POST successful."
            );
#endif

            var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseChannelFollowers>(
                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );

            foreach (var data in twitchResponse.data)
            {
                data.followed_at = data.followed_at.Split(
                    c_twitchUTCSuffix
                )[0u];
                m_channelFollowers.Add(
                    data
                );
            }

            string pageId = twitchResponse.pagination.cursor;
            if (pageId != string.Empty)
            {
                RequestFollowers(
                    pageId,
                    false
                );
            }
            else
            {
#if DEBUG
                GD.Print(
                    $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Number of Followers: {m_channelFollowers.Count}."
                );
#endif

                FollowersRetrieved?.Invoke(
                    m_channelFollowers.ToArray()
                );
            }
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request POST failed with code {responseCode}."
            );
#endif
        }
    }

    private void OnRequestGiftedSubscribersCompleted(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Web request {responseCode} POST successful."
            );
#endif

            var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseUsersSubscribers>(
                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );

            var subscriptionGift = m_pendingSubscriptionGifts.Peek();
            foreach (var data in twitchResponse.data)
            {
                if (data.gifter_id == subscriptionGift.user_id)
                {
                    m_giftedSubscribers.Add(
                        data
                    );

                    if (m_giftedSubscribers.Count == subscriptionGift.total)
                    {
                        break;
                    }
                }
            }

            if (twitchResponse.pagination.cursor != string.Empty || m_giftedSubscribers.Count < subscriptionGift.total)
            {
                RequestGiftedSubscribers(
                    twitchResponse.pagination.cursor,
                    false
                );
            }
            else
            {
#if DEBUG
                GD.Print(
                    $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Number of Gifted Subscribers: {m_giftedSubscribers.Count}."
                );
#endif

                GiftedSubscribersRetrieved?.Invoke(
                    m_giftedSubscribers.ToArray()
                );

                m_pendingSubscriptionGifts.Dequeue();
                if (m_pendingSubscriptionGifts.Count > 0u)
                {
                    RequestGiftedSubscribers(
                        string.Empty
                    );
                }
            }
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Web request POST failed with code {responseCode}."
            );
#endif
        }
    }

    private void OnRequestLatestFollowerCompleted(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request {responseCode} POST successful."
            );
#endif

            var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseChannelFollowers>(
                                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );

            TwitchResponseChannelFollowersData data = twitchResponse.data[0u];
            data.followed_at = data.followed_at.Split(
                c_twitchUTCSuffix
            )[0u];

            m_channelFollowers.Add(
                data
            );
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request POST failed with code {responseCode}."
            );
#endif
        }
    }

    private void OnRequestOAuthCompleted(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
#if DEBUG
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestOAuth)}() - Web request {responseCode} POST successful."
            );
        }
        else
        {
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestOAuth)}() - Web request POST failed with code {responseCode}."
            );
        }
#endif
    }

    private void OnRequestSubscribersCompleted(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Web request {responseCode} POST successful."
            );
#endif

            var twitchResponse = JsonConvert.DeserializeObject<TwitchResponseUsersSubscribers>(
                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );

            foreach (var data in twitchResponse.data)
            {
                m_channelSubscribers.Add(
                    data
                );
            }

            if (twitchResponse.pagination.cursor != string.Empty)
            {
                RequestSubscribers(
                    twitchResponse.pagination.cursor,
                    false
                );
            }
            else
            {
#if DEBUG
                GD.Print(
                    $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Number of Subscribers: {m_channelSubscribers.Count}."
                );
#endif

                SubscribersRetrieved?.Invoke(
                    m_channelSubscribers.ToArray()
                );
            }
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Web request POST failed with code {responseCode}."
            );
#endif
        }
    }

    private void OnRetrievedEventSubSubscriptions(
        long result, 
        long responseCode, 
        string[] headers, 
        byte[] body
    )
    {
        if (
            WasHttpResponseSuccessful(
                responseCode
            )
        )
        {
#if DEBUG
            GD.Print(
                $"{nameof(TwitchManager)}.{nameof(RetrieveEventSubSubscriptions)}() - Web request {responseCode} POST successful."
            );
#endif

            DeleteEventSubSubscriptions(
                Encoding.UTF8.GetString(
                    body,
                    0,
                    body.Length
                )
            );
        }
        else
        {
#if DEBUG
            GD.PrintErr(
                $"{nameof(TwitchManager)}.{nameof(RetrieveEventSubSubscriptions)}() - Web request POST failed with code {responseCode}."
            );
#endif
        }
    }

    private TwitchWebSocketMessage ParseWebSocketMessage(
        byte[] bytes,
        WebSocketReceiveResult result    
    )
    {
        TwitchWebSocketMessage message = JsonConvert.DeserializeObject<TwitchWebSocketMessage>(
            Encoding.UTF8.GetString(
                bytes,
                0,
                result.Count
            )
        );

        if (message != null)
        {
            switch (message.metadata.subscription_type)
            {
                case "channel.chat.notification":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelChatNotification>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.cheer":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelCheer>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.channel_points_custom_reward_redemption.add":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelPointsCustomRewardRedeemed>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.follow":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelFollow>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.raid":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelRaid>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.subscribe":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelSubscribe>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                case "channel.subscription.gift":
                    message = JsonConvert.DeserializeObject<TwitchWebSocketMessageChannelSubscriptionGift>(
                        Encoding.UTF8.GetString(
                            bytes,
                            0,
                            result.Count
                        )
                    );
                    break;

                default:
                    break;
            }
        }
        return message;
    }

    private void ReadWebSocketMessage(
        TwitchWebSocketMessage message
    )
    {
        if (message != null)
        {
            string type = message.metadata.message_type;
            switch (type)
            {
                case "notification":
                    HandleWebSocketMessageNotification(
                        message
                    );
                    break;
                case "revocation":
                    HandleWebSocketMessageRevocation(
                        message
                    );
                    break;
                case "session_keepalive":
                    HandleWebSocketMessageSessionKeepAlive(
                        message
                    );
                    break;
                case "session_reconnect":
                    HandleWebSocketMessageSessionReconnect(
                        message
                    );
                    break;
                case "session_welcome":
                    HandleWebSocketMessageSessionWelcome(
                        message
                    );
                    break;

                default:
                    return;
            }
        }
    }

    private void RegisterEventSubSubscriptions(
        string sessionId    
    )
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
            $"Content-Type: application/json"
        };

        var eventSubTypes = Enum.GetValues<TwitchEventSubSubscriptionType>();
        foreach (var eventSubType in eventSubTypes)
        {
            string payload = string.Empty;
            switch (eventSubType)
            {
                case TwitchEventSubSubscriptionType.ChannelChatNotification:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelChatNotification(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelCheer:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelCheer(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelFollow:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelFollow(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelRaid:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelRaid(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelSubscribe:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelSubscribe(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelSubscriptionGift:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelSubscriptionGift(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed:
                    payload = JsonConvert.SerializeObject(
                        new TwitchRequestEventSubChannelPointsRedemption(
                            $"{TwitchData.AccountId}",
                            $"{sessionId}"
                        )
                    );
                    break;

                case TwitchEventSubSubscriptionType.Unknown:
                default:
                    continue;
            }

            m_httpManager.SendHttpRequest(
                $"{c_urlAPI}/eventsub/subscriptions",
                headers,
                Method.Post,
                payload,
                OnRegisteredEventSubSubscription
            );
        }
    }

    private void RequestChannelPointRewardAdd()
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
            $"Content-Type: application/json"
        };

        var twitchChannelPointRewards = m_twitchChannelPointRewardsManager.TwitchChannelPointRewards;
        foreach (var channelPointReward in twitchChannelPointRewards)
        {
            ChannelPointRewardType channelPointRewardType = channelPointReward.Key;

            if (
                m_channelPointRewardIds.ContainsKey(
                    channelPointRewardType
                )
            )
            {
                continue;
            }

            ChannelPointReward channelPointRewardData = channelPointReward.Value;
            string payload = "{" +
                                                                                        $"\"background_color\":\"{                      channelPointRewardData.background_color}\"," +
                                                                                        $"\"cost\":\"{                                  channelPointRewardData.cost}\"," +
                        (channelPointRewardData.global_cooldown_seconds > 0 ?           $"\"global_cooldown_seconds\":\"{               channelPointRewardData.global_cooldown_seconds}\","               : "") +
                        (channelPointRewardData.is_enabled == false ?                   $"\"is_enabled\":\"{                            channelPointRewardData.is_enabled}\","                            : "") +                          
                        (channelPointRewardData.is_global_cooldown_enabled ?            $"\"is_global_cooldown_enabled\":\"{            channelPointRewardData.is_global_cooldown_enabled}\","            : "") +
                        (channelPointRewardData.is_max_per_stream_enabled ?             $"\"is_max_per_stream_enabled\":\"{             channelPointRewardData.is_max_per_stream_enabled}\","             : "") +
                        (channelPointRewardData.is_max_per_user_per_stream_enabled ?    $"\"is_max_per_user_per_stream_enabled\":\"{    channelPointRewardData.is_max_per_user_per_stream_enabled}\","    : "") +
                        (channelPointRewardData.is_user_input_required ?                $"\"is_user_input_required\":\"{                channelPointRewardData.is_user_input_required}\","                : "") +
                        (channelPointRewardData.is_max_per_stream_enabled ?             $"\"max_per_stream\":\"{                        channelPointRewardData.max_per_stream}\","                        : "") +
                        (channelPointRewardData.is_max_per_user_per_stream_enabled ?    $"\"max_per_user_per_stream\":\"{               channelPointRewardData.max_per_user_per_stream}\","               : "") +
                        (channelPointRewardData.prompt != string.Empty ?                $"\"prompt\":\"{                                channelPointRewardData.prompt}\","                                : "") +                                                             
                        (channelPointRewardData.should_redemptions_skip_request_queue ? $"\"should_redemptions_skip_request_queue\":\"{ channelPointRewardData.should_redemptions_skip_request_queue}\"," : "") +
                                                                                        $"\"title\":\"{                                 channelPointRewardData.title}\"" +
            "}";
            m_httpManager.SendHttpRequest(
                $"{c_urlAPI}/channel_points/custom_rewards?broadcaster_id={TwitchData.AccountId}",
                headers,
                Method.Post,
                payload,
                OnRequestChannelPointRewardAdded
            );
        }

    }

    private void RequestChannelPointRewardDelete()
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
        };

        var customChannelPointRewardTypes = Enum.GetValues<ChannelPointRewardType>();
        foreach (var customChannelPointRewardType in customChannelPointRewardTypes)
        {
            m_httpManager.SendHttpRequest(
                $"{c_urlAPI}/channel_points/custom_rewards?broadcaster_id={TwitchData.AccountId}&id={m_channelPointRewardIds[customChannelPointRewardType]}",
                headers,
                Method.Delete,
                string.Empty,
                OnRequestChannelPointRewardDeleted
            );
        }
    }

    private void RequestChannelPointRewardPatchRedeemCanceled()
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
            $"Content-Type: application/json"
        };
        string payload = "{" +
            $"\"status\":\"CANCELED\"" +
        "}";

        var pendingRewards = m_twitchChannelPointRewardsManager.GetPendingRewards();
        foreach (var pendingReward in pendingRewards)
        {
            m_httpManager.SendHttpRequest(
                $"{c_urlAPI}/channel_points/custom_rewards/redemptions?broadcaster_id={TwitchData.AccountId}&reward_id={m_channelPointRewardIds[pendingReward.twitchChannelPointRewardsType]}&id={pendingReward.id}",
                headers,
                Method.Patch,
                payload,
                OnRequestChannelPointRewardRedeemCanceled
            );
        }
    }

    private void RequestChannelPointRewardPatchRedeemSetPlaylist(
        string id,
        bool redeemed
    )
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
            $"Content-Type: application/json"
        };
        string payload = "{" +
            $"\"status\":\"{(redeemed ? "FULFILLED" : "CANCELED")}\"" +
        "}";
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/channel_points/custom_rewards/redemptions?broadcaster_id={TwitchData.AccountId}&reward_id={m_channelPointRewardIds[ChannelPointRewardType.CommandSetPlaylist]}&id={id}",
            headers,
            Method.Patch,
            payload,
            OnRequestChannelPointRewardRedeemedSetPlaylist
        );
    }

    private void RequestChannelPointRewardUpdateSetPlaylist()
    {
        string availablePlaylists = string.Empty;
        var currentPlaylistType = m_audioManager.GetCurrentPlaylistType();
        var playlistTypes = Enum.GetValues<PlaylistType>();
        for (int i = 0; i < playlistTypes.Length; i++)
        {
            if (playlistTypes[i] == currentPlaylistType || playlistTypes[i] == PlaylistType.Count)
            {
                continue;
            }

            availablePlaylists += playlistTypes[i].ToString();

            PlaylistType nextPlaylistType = playlistTypes[i + 1u];
            if (nextPlaylistType == PlaylistType.Count)
            {
                break;
            }
            else if (nextPlaylistType != currentPlaylistType)
            {
                availablePlaylists += ", ";
            }
        }

        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}",
            $"Content-Type: application/json"
        };
        string payload = "{" +
            $"\"prompt\":\"Type !setplaylist followed by the playlist name in chat. Available Playlists: {availablePlaylists}\"" +
        "}";
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/channel_points/custom_rewards?broadcaster_id={TwitchData.AccountId}&id={m_channelPointRewardIds[ChannelPointRewardType.CommandSetPlaylist]}",
            headers,
            Method.Patch,
            payload,
            OnRequestChannelPointRewardUpdatedSetPlaylist
        );
    }

    private void RequestFollowers(
        string pageId,
        bool clear = true
    )
    {
        if (clear)
        {
            m_channelFollowers.Clear();
        }

        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/channels/followers?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
            headers,
            Method.Get,
            string.Empty,
            OnRequestFollowersCompleted
        );
    }

    private void RequestGiftedSubscribers(
        string pageId,
        bool clear = true
    )
    {
        if (clear)
        {
            m_giftedSubscribers.Clear();
        }

        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
            headers,
            Method.Get,
            string.Empty,
            OnRequestGiftedSubscribersCompleted
        );
    }

    private void RequestOAuth()
    {
        string[] headers = new string[]
        {
             $"application/x-www-form-urlencoded"
        };
        string payload = $"client_id={TwitchData.ClientId}&" +
                         $"client_secret={TwitchData.ClientSecret}&" +
                         $"grant_type=client_credentials";

        m_httpManager.SendHttpRequest(
            $"{c_urlOAuth}",
            headers,
            Method.Post,
            payload,
            OnRequestOAuthCompleted
        );
    }

    private void RequestLatestFollower()
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=1",
            headers,
            Method.Get,
            string.Empty,
            OnRequestLatestFollowerCompleted
        );
    }

    private void RequestSubscribers(
        string pageId, 
        bool clear = true
    )
    {
        if (clear)
        {
            m_channelSubscribers.Clear();
        }

        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
            headers,
            Method.Get,
            string.Empty,
            OnRequestSubscribersCompleted
        );
    }

    private void RetrieveEventSubSubscriptions()
    {
        string[] headers = new string[]
        {
            $"Authorization: Bearer {TwitchData.AccountAccessToken}",
            $"Client-Id: {TwitchData.ClientId}"
        };
        m_httpManager.SendHttpRequest(
            $"{c_urlAPI}/eventsub/subscriptions",
            headers,
            Method.Get,
            string.Empty,
            OnRetrievedEventSubSubscriptions
        );
    }
}