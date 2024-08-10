
namespace Overlay
{
	using Godot;
	using System;
	using System.Collections.Generic;
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierController : Control
	{
		public override void _EnterTree()
		{
			RetrieveNotifiers();
			RegisterForNotiferCompletions();
			RegisterForTwitchEvents();
		}

		public override void _Ready()
		{
			StartNotifying();
		}

		private enum AutomaticNotifierType : uint
		{
			Discord = 0u,
			RecentFollowers,
			RecentSubscribers,
			Twitch,
			YouTube,
			Count
		}

		private enum EventNotifierType : uint
		{
			NewCheer = 0u,
			NewFollower,
			NewGiftedSubscription,
			NewSubscriber
		}

        private readonly Dictionary<AutomaticNotifierType, NotifierControllerBase> m_automaticNotifierControllers = new();
        private readonly Queue<EventNotifierType> m_eventNotifierTypeQueue = new();
        private readonly Random m_random = new();

        private NotifierControllerTextScrollerEventNewCheer m_notifierControllerNewCheer = null;
		private NotifierControllerTextScrollerEventNewFollower m_notifierControllerNewFollower = null;
		private NotifierControllerTextScrollerEventNewGiftedSubscription m_notifierControllerNewGiftedSubscription = null;
		private NotifierControllerTextScrollerEventNewSubscriber m_notifierControllerNewSubscriber = null;

		private NotifierControllerTextScrollerRecentEventFollowers m_notifierControllerRecentFollowers = null;
		private NotifierControllerTextScrollerRecentEventSubscribers m_notifierControllerRecentSubscribers = null;

		private NotifierControllerTextScrollerSocialDiscord m_notifierControllerDiscord = null;
		private NotifierControllerTextScrollerSocialTwitch m_notifierControllerTwitch = null;
		private NotifierControllerTextScrollerSocialYoutube m_notifierControllerYouTube = null;

        private AutomaticNotifierType m_currentAutomaticNotifierType = AutomaticNotifierType.Discord;
		private NotifierControllerBase m_currentNotifierController = null;

		private void OnChannelCheered(
			TwitchWebSocketMessagePayloadEventChannelCheer payload
		)
		{
			m_eventNotifierTypeQueue.Enqueue(
				item: EventNotifierType.NewCheer
			);
		}

		private void OnChannelFollowed(
			TwitchWebSocketMessagePayloadEventChannelFollow payload
		)
		{
			m_eventNotifierTypeQueue.Enqueue(
                item: EventNotifierType.NewFollower
			);
		}

		private void OnChannelSubscriptionGifted(
			TwitchWebSocketMessagePayloadEventChannelSubscriptionGift payload
		)
		{
			m_eventNotifierTypeQueue.Enqueue(
                item: EventNotifierType.NewGiftedSubscription
			);
		}

		private void OnChannelSubscribed(
			TwitchWebSocketMessagePayloadEventChannelSubscribe payload
		)
		{
			m_eventNotifierTypeQueue.Enqueue(
                item: EventNotifierType.NewSubscriber
			);
		}

		private void OnNotificationCompleted()
		{
			if (m_eventNotifierTypeQueue.Count > 0u)
			{
				var eventNotifierType = m_eventNotifierTypeQueue.Dequeue();
				switch (eventNotifierType)
				{
					case EventNotifierType.NewFollower:
						m_currentNotifierController = m_notifierControllerNewFollower;
						break;
					case EventNotifierType.NewGiftedSubscription:
						m_currentNotifierController = m_notifierControllerNewGiftedSubscription;
						break;
					case EventNotifierType.NewSubscriber:
						m_currentNotifierController = m_notifierControllerNewSubscriber;
						break;

					default:
						SelectNextAutomaticNotifierController();
						break;
				}
			}
			else
			{
				SelectNextAutomaticNotifierController();
			}

			m_currentNotifierController.StartNotification();
		}

        private void RetrieveNotifiers()
        {
            m_notifierControllerDiscord = GetNode<NotifierControllerTextScrollerSocialDiscord>(
                path: "NotifierControllerDiscord"
            );
            m_notifierControllerNewCheer = GetNode<NotifierControllerTextScrollerEventNewCheer>(
                path: "NotifierControllerNewCheer"
            );
            m_notifierControllerNewFollower = GetNode<NotifierControllerTextScrollerEventNewFollower>(
                path: "NotifierControllerNewFollower"
            );
            m_notifierControllerNewGiftedSubscription = GetNode<NotifierControllerTextScrollerEventNewGiftedSubscription>(
                path: "NotifierControllerNewGiftedSubscription"
            );
            m_notifierControllerNewSubscriber = GetNode<NotifierControllerTextScrollerEventNewSubscriber>(
                path: "NotifierControllerNewSubscriber"
            );
            m_notifierControllerRecentFollowers = GetNode<NotifierControllerTextScrollerRecentEventFollowers>(
                path: "NotifierControllerRecentFollowers"
            );
            m_notifierControllerRecentSubscribers = GetNode<NotifierControllerTextScrollerRecentEventSubscribers>(
                path: "NotifierControllerRecentSubscribers"
            );
            m_notifierControllerTwitch = GetNode<NotifierControllerTextScrollerSocialTwitch>(
                path: "NotifierControllerTwitch"
            );
            m_notifierControllerYouTube = GetNode<NotifierControllerTextScrollerSocialYoutube>(
                path: "NotifierControllerYouTube"
            );

            m_automaticNotifierControllers.Add(
                key: AutomaticNotifierType.Discord,
                value: m_notifierControllerDiscord
            );
            m_automaticNotifierControllers.Add(
                key: AutomaticNotifierType.RecentFollowers,
                value: m_notifierControllerRecentFollowers
            );
            m_automaticNotifierControllers.Add(
                key: AutomaticNotifierType.RecentSubscribers,
                value: m_notifierControllerRecentSubscribers
            );
            m_automaticNotifierControllers.Add(
                key: AutomaticNotifierType.Twitch,
                value: m_notifierControllerTwitch
            );
            m_automaticNotifierControllers.Add(
                key: AutomaticNotifierType.YouTube,
                value: m_notifierControllerYouTube
            );
        }

        private void RegisterForNotiferCompletions()
        {
            m_notifierControllerDiscord.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerNewCheer.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerNewFollower.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerNewGiftedSubscription.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerNewSubscriber.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerRecentFollowers.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerRecentSubscribers.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerTwitch.CompletedNotification += OnNotificationCompleted;
            m_notifierControllerYouTube.CompletedNotification += OnNotificationCompleted;
        }

        private void RegisterForTwitchEvents()
        {
            var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );

            twitchManager.ChannelCheered += OnChannelCheered;
            twitchManager.ChannelFollowed += OnChannelFollowed;
            twitchManager.ChannelSubscriptionGifted += OnChannelSubscriptionGifted;
            twitchManager.ChannelSubscribed += OnChannelSubscribed;
        }

        private void SelectNextAutomaticNotifierController()
		{
			var currentNotifierType = m_currentAutomaticNotifierType;
			while (
				currentNotifierType.Equals(
					obj: m_currentAutomaticNotifierType
				) is true
			)
			{
				m_currentAutomaticNotifierType = (AutomaticNotifierType)m_random.Next(
					minValue: (int)AutomaticNotifierType.Discord,
					maxValue: (int)AutomaticNotifierType.Count
				);
			}

			m_currentNotifierController = m_automaticNotifierControllers[m_currentAutomaticNotifierType];
		}

        private void StartNotifying()
        {
            m_currentAutomaticNotifierType = (AutomaticNotifierType)m_random.Next(
                minValue: (int)AutomaticNotifierType.Discord,
                maxValue: (int)AutomaticNotifierType.Count
            );
            m_currentNotifierController = m_automaticNotifierControllers[m_currentAutomaticNotifierType];
            m_currentNotifierController.StartNotification();
        }
    }
}