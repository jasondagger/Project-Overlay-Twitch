
namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
	using System.Net.WebSockets;
	using System.Text;
	using System.Text.Json;
	using System.Threading.Tasks;
	using static Godot.HttpClient;
	using ChannelPointReward = TwitchChannelPointRewardsManager.ChannelPointReward;
	using ChannelPointRewardType = TwitchChannelPointRewardsManager.ChannelPointRewardsType;
	using NodeType = NodeDirectory.NodeType;

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

			TwitchData.Load();
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
				var type = message.Type;

				switch (type)
				{
					case TwitchEventSubSubscriptionType.ChannelChatNotification:
						var messageChannelNotification = message as TwitchMessageChannelChatNotification;
						ChannelChatNotification?.Invoke(
                            obj: messageChannelNotification.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelCheer:
						var messageChannelCheer = message as TwitchMessageChannelCheer;
						ChannelCheered?.Invoke(
                            obj: messageChannelCheer.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelFollow:
						var messageChannelFollow = message as TwitchMessageChannelFollow;
						ChannelFollowed?.Invoke(
                            obj: messageChannelFollow.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed:
						var messageChannelPointsCustomRewardRedeemed = message as TwitchMessageChannelPointsCustomRewardRedeemed;
						ChannelPointsCustomRewardRedeemed?.Invoke(
                            obj: messageChannelPointsCustomRewardRedeemed.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelRaid:
						var messageChannelRaid = message as TwitchMessageChannelRaid;
						ChannelRaided?.Invoke(
                            obj: messageChannelRaid.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelSubscribe:
						var messageChannelSubscribe = message as TwitchMessageChannelSubscribe;
						ChannelSubscribed?.Invoke(
                             obj: messageChannelSubscribe.Event
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelSubscriptionGift:
						var messageChannelSubscriptionGift = message as TwitchMessageChannelSubscriptionGift;
						ChannelSubscriptionGifted?.Invoke(
							 obj: messageChannelSubscriptionGift.Event
						);
						break;

					default:
						break;
				}
			}
		}

		public override void _Ready()
		{
			ConnectWebSocket();

			RequestChannelPointRewardAdd();
			RequestFollowers(
				pageId: string.Empty
			);
			RequestSubscribers(
				pageId: string.Empty
			);
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
			{ ChannelPointRewardType.CommandRequestSong,     "dfab89a2-0015-4a1e-9cb2-456fcc5e452b" },
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
			{ ChannelPointRewardType.TextToSpeech,           "fef9d5e7-6482-4bf5-82cf-f63a5cb08118" },
		};

		private struct SubscriptionGift
		{
			public string UserId { get; set; } = string.Empty;
			public int Total { get; set; } = 0;

			public SubscriptionGift(
				string userId,
				int total
			)
			{
				this.UserId = userId;
				this.Total = total;
			}
		}

        private readonly List<TwitchResponseUsersSubscribersData> m_channelSubscribers = new();
        private readonly List<TwitchResponseChannelFollowersData> m_channelFollowers = new();
        private readonly List<TwitchResponseUsersSubscribersData> m_giftedSubscribers = new();
		private readonly Queue<TwitchMessage> m_messageQueue = new();
        private readonly Queue<SubscriptionGift> m_pendingSubscriptionGifts = new();
        private readonly ClientWebSocket m_webSocket = new();

        private AudioManager m_audioManager = null;
		private HttpManager m_httpManager = null;
		private TwitchChannelPointRewardsManager m_twitchChannelPointRewardsManager = null;
		private bool m_shutdown = false;

		private void RetrieveResources()
		{
			m_audioManager = GetNode<AudioManager>(
				path: NodeDirectory.NodePaths[NodeType.AudioManager]
			);
			m_httpManager = GetNode<HttpManager>(
                path: NodeDirectory.NodePaths[NodeType.HttpManager]
			);
			m_twitchChannelPointRewardsManager = GetNode<TwitchChannelPointRewardsManager>(
                path: NodeDirectory.NodePaths[NodeType.TwitchChannelPointRewardsManager]
			);
		}

		private async void ConnectWebSocket()
		{
			await Task.Run(
				function: async () =>
				{
					// connect to Twitch websocket
					var uri = new Uri(
						uriString: c_webSocketAddress
					);

					await m_webSocket.ConnectAsync(
						uri: uri,
                        cancellationToken: default
                    );

#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchManager)}.{nameof(ConnectWebSocket)}() - Web socket connect successful."
					);
#endif

					while (m_shutdown is false)
					{
						if (m_webSocket.State is WebSocketState.Open)
						{
							var bytes = new byte[4096u];
							var result = await m_webSocket.ReceiveAsync(
								buffer: bytes,
								cancellationToken: default
							);

							// https://dev.twitch.tv/docs/eventsub/handling-websocket-events/
							var webSocketMessage = ParseWebSocketMessage(
								bytes: bytes,
								result: result
							);
							ReadWebSocketMessage(
								message: webSocketMessage
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
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};

			// delete all existing event subscriptions
			var twitchResponse = JsonSerializer.Deserialize<TwitchResponseEventSubSubscriptions>(
				json: json
			);
			foreach (var data in twitchResponse.Data)
			{
				m_httpManager.SendHttpRequest(
					url: $"{c_urlAPI}/eventsub/subscriptions?id={data.Id}",
					headers: headers,
					method: Method.Delete,
					json: string.Empty,
					requestCompletedHandler: OnDeletedEventSubSubscription
				);
			}
		}

		private void HandleWebSocketMessageNotification(
			TwitchWebSocketMessage message
		)
		{
			switch (message.Metadata.SubscriptionType)
			{
				case "channel.chat.notification":
					HandleWebSocketMessageChannelChatNotification(
						message: message as TwitchWebSocketMessageChannelChatNotification	
					);
                    break;

				case "channel.cheer":
                    HandleWebSocketMessageChannelCheer(
						message: message as TwitchWebSocketMessageChannelCheer
					);
					break;

				case "channel.follow":
					HandleWebSocketMessageChannelFollow(
						message: message as TwitchWebSocketMessageChannelFollow
					);
					break;

				case "channel.channel_points_custom_reward_redemption.add":
                    HandleWebSocketMessageChannelPointsCustomRewardRedeemed(
						message: message as TwitchWebSocketMessageChannelPointsCustomRewardRedeemed
					);
					break;

				case "channel.raid":
					HandleWebSocketMessageChannelRaid(
						message: message as TwitchWebSocketMessageChannelRaid
					);
					break;

				case "channel.subscribe":
					HandleWebSocketMessageChannelSubscribe(
						message: message as TwitchWebSocketMessageChannelSubscribe
                    );
					break;

				case "channel.subscription.gift":
					HandleWebSocketMessageChannelSubscriptionGift(
						message: message as TwitchWebSocketMessageChannelSubscriptionGift
					);
                    break;

				default:
					return;
			}
		}

		private void HandleWebSocketMessageChannelChatNotification(
            TwitchWebSocketMessageChannelChatNotification message
        )
		{
            var payloadChannelChatNotification = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelChatNotification(
                    @event: payloadChannelChatNotification.Event
                )
            );
        }

		private void HandleWebSocketMessageChannelCheer(
            TwitchWebSocketMessageChannelCheer message
        )
        {
            var payloadChannelCheer = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelCheer(
                    @event: payloadChannelCheer.Event
                )
            );
        }

		private void HandleWebSocketMessageChannelFollow(
            TwitchWebSocketMessageChannelFollow message
        )
        {
            var payloadChannelFollow = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelFollow(
                    @event: payloadChannelFollow.Event
                )
            );
            RequestLatestFollower();
        }

		private void HandleWebSocketMessageChannelPointsCustomRewardRedeemed(
            TwitchWebSocketMessageChannelPointsCustomRewardRedeemed message
        )
        {
            var payloadChannelPointsRedeemed = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelPointsCustomRewardRedeemed(
                    @event: payloadChannelPointsRedeemed.Event
                )
            );
        }

		private void HandleWebSocketMessageChannelRaid(
            TwitchWebSocketMessageChannelRaid message
        )
        {
            var payloadChannelRaid = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelRaid(
                    @event: payloadChannelRaid.Event
                )
            );
        }

		private void HandleWebSocketMessageChannelSubscribe(
            TwitchWebSocketMessageChannelSubscribe message
        )
        {
            var payloadChannelSubscribe = message.Payload;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelSubscribe(
                    @event: payloadChannelSubscribe.Event
                )
            );
            RequestSubscribers(
                pageId: string.Empty
            );
        }

		private void HandleWebSocketMessageChannelSubscriptionGift(
            TwitchWebSocketMessageChannelSubscriptionGift message
        )
        {
            var payloadChannelSubscriptionGift = message.Payload;
            var eventChannelSubscriptionGift = payloadChannelSubscriptionGift.Event;
            m_messageQueue.Enqueue(
                item: new TwitchMessageChannelSubscriptionGift(
                    @event: eventChannelSubscriptionGift
                )
            );

            m_pendingSubscriptionGifts.Enqueue(
                item: new(
                    userId: eventChannelSubscriptionGift.UserId,
                    total: eventChannelSubscriptionGift.Total ?? 0
                )
            );
            if (m_pendingSubscriptionGifts.Count is 1)
            {
                RequestGiftedSubscribers(
                    pageId: string.Empty
                );
            }

            RequestSubscribers(
                pageId: string.Empty
            );
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
				message.Payload.Session.Id
			);
		}

		private static bool WasHttpResponseSuccessful(
			long responseCode
		)
		{
			return responseCode >= 200u && responseCode < 300u;
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
					what: $"{nameof(TwitchManager)}.{nameof(RegisterEventSubSubscriptions)}() - Web request {responseCode} POST successful."
				);
			}
			else
			{
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RegisterEventSubSubscriptions)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardAdd)}() - Web request {responseCode} POST successful."
				);
#endif

				var twitchResponse = JsonSerializer.Deserialize<TwitchResponseCustomRewardCreate>(
					json: Encoding.UTF8.GetString(
						bytes: body,
						index: 0,
						count: body.Length
					)
				);

				var channelRewardType = m_twitchChannelPointRewardsManager.GetRewardTypeByName(
					rewardName: twitchResponse.Data[0].Title
				);
				m_channelPointRewardIds[channelRewardType] = twitchResponse.Data[0].Id;

#if DEBUG
				GD.Print(
					what: $"{channelRewardType} Id: {m_channelPointRewardIds[channelRewardType]}"
				);
#endif
			}
			else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardAdd)}() - Web request POST failed with {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardDelete)}() - Web request {responseCode} POST successful."
				);
			}
			else
			{
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardDelete)}() - Web request POST failed with {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemCanceled)}() - Web request {responseCode} POST successful."
				);
			}
			else
			{
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestChannelPointRewardPatchRedeemCanceled)}() - Web request POST failed with {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request {responseCode} POST successful."
				);
#endif

				var twitchResponse = JsonSerializer.Deserialize<TwitchResponseChannelFollowers>(
					json: Encoding.UTF8.GetString(
						bytes: body,
						index: 0,
						count: body.Length
					)
				);

				foreach (var data in twitchResponse.Data)
				{
					data.FollowedAt = data.FollowedAt.Split(
						separator: c_twitchUTCSuffix
					)[0u];
					m_channelFollowers.Add(
						item: data
					);
				}

				var pageId = twitchResponse.Pagination.Cursor;
				if (pageId != string.Empty)
				{
					RequestFollowers(
						pageId: pageId,
						clear: false
					);
				}
				else
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Number of Followers: {m_channelFollowers.Count}."
					);
#endif

					FollowersRetrieved?.Invoke(
						obj: m_channelFollowers.ToArray()
					);
				}
			}
			else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Web request {responseCode} POST successful."
				);
#endif

				var twitchResponse = JsonSerializer.Deserialize<TwitchResponseUsersSubscribers>(
					json: Encoding.UTF8.GetString(
						bytes: body,
						index: 0,
						count: body.Length
					)
				);

				var subscriptionGift = m_pendingSubscriptionGifts.Peek();
				foreach (var data in twitchResponse.Data)
				{
					if (data.GifterId == subscriptionGift.UserId)
					{
						m_giftedSubscribers.Add(
							item: data
						);

						if (m_giftedSubscribers.Count == subscriptionGift.Total)
						{
							break;
						}
					}
				}

				if (
					twitchResponse.Pagination.Cursor != string.Empty || 
					m_giftedSubscribers.Count < subscriptionGift.Total
				)
				{
					RequestGiftedSubscribers(
						pageId: twitchResponse.Pagination.Cursor,
						clear: false
					);
				}
				else
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Number of Gifted Subscribers: {m_giftedSubscribers.Count}."
					);
#endif

					GiftedSubscribersRetrieved?.Invoke(
						obj: m_giftedSubscribers.ToArray()
					);

					m_pendingSubscriptionGifts.Dequeue();
					if (m_pendingSubscriptionGifts.Count > 0u)
					{
						RequestGiftedSubscribers(
							pageId: string.Empty
						);
					}
				}
			}
			else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestGiftedSubscribers)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request {responseCode} POST successful."
				);
#endif

				var twitchResponse = JsonSerializer.Deserialize<TwitchResponseChannelFollowers>(
					json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
				);

				var data = twitchResponse.Data[0u];
				data.FollowedAt = data.FollowedAt.Split(
					separator: c_twitchUTCSuffix
				)[0u];

				m_channelFollowers.Add(
					item: data
				);
			}
			else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestFollowers)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestOAuth)}() - Web request {responseCode} POST successful."
				);
			}
			else
			{
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestOAuth)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Web request {responseCode} POST successful."
				);
#endif

				var twitchResponse = JsonSerializer.Deserialize<TwitchResponseUsersSubscribers>(
					json: Encoding.UTF8.GetString(
                        bytes: body,
                        index: 0,
                        count: body.Length
                    )
				);

				foreach (var data in twitchResponse.Data)
				{
					m_channelSubscribers.Add(
						item: data
					);
				}

				if (twitchResponse.Pagination.Cursor != string.Empty)
				{
					RequestSubscribers(
						pageId: twitchResponse.Pagination.Cursor,
						clear: false
					);
				}
				else
				{
#if DEBUG
					GD.Print(
						what: $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Number of Subscribers: {m_channelSubscribers.Count}."
					);
#endif

					SubscribersRetrieved?.Invoke(
						obj: m_channelSubscribers.ToArray()
					);
				}
			}
			else
			{
#if DEBUG
				GD.PrintErr(
					what: $"{nameof(TwitchManager)}.{nameof(RequestSubscribers)}() - Web request POST failed with code {responseCode}."
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
					responseCode: responseCode
				)
			)
			{
#if DEBUG
				GD.Print(
					what: $"{nameof(TwitchManager)}.{nameof(RetrieveEventSubSubscriptions)}() - Web request {responseCode} POST successful."
				);
#endif

				DeleteEventSubSubscriptions(
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
					what: $"{nameof(TwitchManager)}.{nameof(RetrieveEventSubSubscriptions)}() - Web request POST failed with code {responseCode}."
				);
#endif
			}
		}

		private static TwitchWebSocketMessage ParseWebSocketMessage(
			byte[] bytes,
			WebSocketReceiveResult result
		)
		{
            var message = JsonSerializer.Deserialize<TwitchWebSocketMessage>(
                json: Encoding.UTF8.GetString(
                    bytes: bytes,
                    index: 0,
                    count: result.Count
                )
            );

			if (message is not null)
			{
				switch (message.Metadata.SubscriptionType)
				{
					case "channel.chat.notification":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelChatNotification>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.cheer":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelCheer>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.channel_points_custom_reward_redemption.add":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelPointsCustomRewardRedeemed>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.follow":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelFollow>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.raid":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelRaid>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.subscribe":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelSubscribe>(
                            json: Encoding.UTF8.GetString(
                                bytes: bytes,
                                index: 0,
                                count: result.Count
                            )
						);
						break;

					case "channel.subscription.gift":
						message = JsonSerializer.Deserialize<TwitchWebSocketMessageChannelSubscriptionGift>(
                            json: Encoding.UTF8.GetString(
								bytes: bytes,
								index: 0,
								count: result.Count
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
			if (message is not null)
			{
				var type = message.Metadata.MessageType;
				switch (type)
				{
					case "notification":
						HandleWebSocketMessageNotification(
                            message: message
                        );
						break;
					case "revocation":
						HandleWebSocketMessageRevocation(
                            message: message
                        );
						break;
					case "session_keepalive":
						HandleWebSocketMessageSessionKeepAlive(
                            message: message
                        );
						break;
					case "session_reconnect":
						HandleWebSocketMessageSessionReconnect(
                            message: message
                        );
						break;
					case "session_welcome":
						HandleWebSocketMessageSessionWelcome(
							message: message
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
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}",
				$"Content-Type: application/json"
			};

			var eventSubTypes = Enum.GetValues<TwitchEventSubSubscriptionType>();
			foreach (var eventSubType in eventSubTypes)
			{
				var payload = string.Empty;
				switch (eventSubType)
				{
					case TwitchEventSubSubscriptionType.ChannelChatNotification:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelChatNotification(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelCheer:
						payload = JsonSerializer.Serialize(
                            value: new TwitchRequestEventSubChannelCheer(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelFollow:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelFollow(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelRaid:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelRaid(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelSubscribe:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelSubscribe(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelSubscriptionGift:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelSubscriptionGift(
                                userId: $"{TwitchData.AccountId}",
                                sessionId: $"{sessionId}"
                            )
						);
						break;

					case TwitchEventSubSubscriptionType.ChannelPointsCustomRewardRedeemed:
						payload = JsonSerializer.Serialize(
							value: new TwitchRequestEventSubChannelPointsRedemption(
								userId: $"{TwitchData.AccountId}",
								sessionId: $"{sessionId}"
							)
						);
						break;

					case TwitchEventSubSubscriptionType.Unknown:
					default:
						continue;
				}

				m_httpManager.SendHttpRequest(
					url: $"{c_urlAPI}/eventsub/subscriptions",
					headers: headers,
					method: Method.Post,
					json: payload,
					requestCompletedHandler: OnRegisteredEventSubSubscription
				);
			}
		}

		private void RequestChannelPointRewardAdd()
		{
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}",
				$"Content-Type: application/json"
			};

			var twitchChannelPointRewards = m_twitchChannelPointRewardsManager.TwitchChannelPointRewards;
			foreach (var channelPointReward in twitchChannelPointRewards)
			{
				var channelPointRewardType = channelPointReward.Key;
				if (
					m_channelPointRewardIds.ContainsKey(
						key: channelPointRewardType
					)
				)
				{
					continue;
				}

				var channelPointRewardData = channelPointReward.Value;
				var payload = "{" +
																					$"\"background_color\":\"{channelPointRewardData.background_color}\"," +
																					$"\"cost\":\"{channelPointRewardData.cost}\"," +
					(channelPointRewardData.global_cooldown_seconds > 0 ?			$"\"global_cooldown_seconds\":\"{channelPointRewardData.global_cooldown_seconds}\"," : "") +
					(channelPointRewardData.is_enabled == false ?					$"\"is_enabled\":\"{channelPointRewardData.is_enabled}\"," : "") +
					(channelPointRewardData.is_global_cooldown_enabled ?			$"\"is_global_cooldown_enabled\":\"{channelPointRewardData.is_global_cooldown_enabled}\"," : "") +
					(channelPointRewardData.is_max_per_stream_enabled ?				$"\"is_max_per_stream_enabled\":\"{channelPointRewardData.is_max_per_stream_enabled}\"," : "") +
					(channelPointRewardData.is_max_per_user_per_stream_enabled ?	$"\"is_max_per_user_per_stream_enabled\":\"{channelPointRewardData.is_max_per_user_per_stream_enabled}\"," : "") +
					(channelPointRewardData.is_user_input_required ?				$"\"is_user_input_required\":\"{channelPointRewardData.is_user_input_required}\"," : "") +
					(channelPointRewardData.is_max_per_stream_enabled ?				$"\"max_per_stream\":\"{channelPointRewardData.max_per_stream}\"," : "") +
					(channelPointRewardData.is_max_per_user_per_stream_enabled ?	$"\"max_per_user_per_stream\":\"{channelPointRewardData.max_per_user_per_stream}\"," : "") +
					(channelPointRewardData.prompt != string.Empty ?				$"\"prompt\":\"{channelPointRewardData.prompt}\"," : "") +
					(channelPointRewardData.should_redemptions_skip_request_queue ? $"\"should_redemptions_skip_request_queue\":\"{channelPointRewardData.should_redemptions_skip_request_queue}\"," : "") +
																					$"\"title\":\"{channelPointRewardData.title}\"" +
				"}";
				m_httpManager.SendHttpRequest(
					url: $"{c_urlAPI}/channel_points/custom_rewards?broadcaster_id={TwitchData.AccountId}",
					headers: headers,
					method: Method.Post,
					json: payload,
					requestCompletedHandler: OnRequestChannelPointRewardAdded
				);
			}

		}

		private void RequestChannelPointRewardDelete()
		{
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}",
			};

			var customChannelPointRewardTypes = Enum.GetValues<ChannelPointRewardType>();
			foreach (var customChannelPointRewardType in customChannelPointRewardTypes)
			{
				m_httpManager.SendHttpRequest(
                    url: $"{c_urlAPI}/channel_points/custom_rewards?broadcaster_id={TwitchData.AccountId}&id={m_channelPointRewardIds[customChannelPointRewardType]}",
                    headers: headers,
                    method: Method.Delete,
                    json: string.Empty,
                    requestCompletedHandler: OnRequestChannelPointRewardDeleted
                );
			}
		}

		private void RequestChannelPointRewardPatchRedeemCanceled()
		{
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}",
				$"Content-Type: application/json"
			};
			var payload = "{" +
				$"\"status\":\"CANCELED\"" +
			"}";

			var pendingRewards = m_twitchChannelPointRewardsManager.GetPendingRewards();
			foreach (var pendingReward in pendingRewards)
			{
				m_httpManager.SendHttpRequest(
                    url: $"{c_urlAPI}/channel_points/custom_rewards/redemptions?broadcaster_id={TwitchData.AccountId}&reward_id={m_channelPointRewardIds[pendingReward.twitchChannelPointRewardsType]}&id={pendingReward.id}",
                    headers: headers,
                    method: Method.Patch,
                    json: payload,
                    requestCompletedHandler: OnRequestChannelPointRewardRedeemCanceled
                );
			}
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

			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};
			m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/channels/followers?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
				headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestFollowersCompleted
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

			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};
			m_httpManager.SendHttpRequest(
				url: $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
				headers: headers,
				method: Method.Get,
				json: string.Empty,
                requestCompletedHandler: OnRequestGiftedSubscribersCompleted
            );
		}

		private void RequestOAuth()
		{
			var headers = new string[]
			{
				$"application/x-www-form-urlencoded"
			};
			var payload = 
				$"client_id={TwitchData.ClientId}&" +
				$"client_secret={TwitchData.ClientSecret}&" +
				$"grant_type=client_credentials";

			m_httpManager.SendHttpRequest(
                url: $"{c_urlOAuth}",
                headers: headers,
                method: Method.Post,
                json: payload,
                requestCompletedHandler: OnRequestOAuthCompleted
            );
		}

		private void RequestLatestFollower()
		{
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};
			m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=1",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestLatestFollowerCompleted
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

			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};
			m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/subscriptions?broadcaster_id={TwitchData.AccountId}&first=100&after={pageId}",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRequestSubscribersCompleted
            );
		}

		private void RetrieveEventSubSubscriptions()
		{
			var headers = new string[]
			{
				$"Authorization: Bearer {TwitchData.AccountAccessToken}",
				$"Client-Id: {TwitchData.ClientId}"
			};
			m_httpManager.SendHttpRequest(
                url: $"{c_urlAPI}/eventsub/subscriptions",
                headers: headers,
                method: Method.Get,
                json: string.Empty,
                requestCompletedHandler: OnRetrievedEventSubSubscriptions
            );
		}
	}
}