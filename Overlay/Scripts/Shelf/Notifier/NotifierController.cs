using Godot;
using System;
using System.Collections.Generic;
using NodeType = NodeDirectory.NodeType;

public sealed partial class NotifierController : Node
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

    private NotifierControllerTextScrollerEventNewCheer m_notifierControllerNewCheer = null;
    private NotifierControllerTextScrollerEventNewFollower m_notifierControllerNewFollower = null;
    private NotifierControllerTextScrollerEventNewGiftedSubscription m_notifierControllerNewGiftedSubscription = null;
    private NotifierControllerTextScrollerEventNewSubscriber m_notifierControllerNewSubscriber = null;

    private NotifierControllerTextScrollerRecentEventFollowers m_notifierControllerRecentFollowers = null;
    private NotifierControllerTextScrollerRecentEventSubscribers m_notifierControllerRecentSubscribers = null;

    private NotifierControllerTextScrollerSocialDiscord m_notifierControllerDiscord = null;
    private NotifierControllerTextScrollerSocialTwitch m_notifierControllerTwitch = null;
    private NotifierControllerTextScrollerSocialYoutube m_notifierControllerYouTube = null;

    private Dictionary<AutomaticNotifierType, NotifierControllerBase> m_automaticNotifierControllers = new();
    private Queue<EventNotifierType> m_eventNotifierTypeQueue = new();
    private AutomaticNotifierType m_currentAutomaticNotifierType = AutomaticNotifierType.Discord;
    private NotifierControllerBase m_currentNotifierController = null;
    private Random m_random = new();

    private void RetrieveNotifiers()
    {
        m_notifierControllerDiscord = GetNode<NotifierControllerTextScrollerSocialDiscord>(
            "NotifierControllerDiscord"
        );
        m_notifierControllerNewCheer = GetNode<NotifierControllerTextScrollerEventNewCheer>(
            "NotifierControllerNewCheer"
        );
        m_notifierControllerNewFollower = GetNode<NotifierControllerTextScrollerEventNewFollower>(
            "NotifierControllerNewFollower"
        );
        m_notifierControllerNewGiftedSubscription = GetNode<NotifierControllerTextScrollerEventNewGiftedSubscription>(
            "NotifierControllerNewGiftedSubscription"
        );
        m_notifierControllerNewSubscriber = GetNode<NotifierControllerTextScrollerEventNewSubscriber>(
            "NotifierControllerNewSubscriber"
        );
        m_notifierControllerRecentFollowers = GetNode<NotifierControllerTextScrollerRecentEventFollowers>(
            "NotifierControllerRecentFollowers"
        );
        m_notifierControllerRecentSubscribers = GetNode<NotifierControllerTextScrollerRecentEventSubscribers>(
            "NotifierControllerRecentSubscribers"
        );
        m_notifierControllerTwitch = GetNode<NotifierControllerTextScrollerSocialTwitch>(
            "NotifierControllerTwitch"
        );
        m_notifierControllerYouTube = GetNode<NotifierControllerTextScrollerSocialYoutube>(
            "NotifierControllerYouTube"
        );

        m_automaticNotifierControllers.Add(
            AutomaticNotifierType.Discord,
            m_notifierControllerDiscord
        );
        m_automaticNotifierControllers.Add(
            AutomaticNotifierType.RecentFollowers,
            m_notifierControllerRecentFollowers
        );
        m_automaticNotifierControllers.Add(
            AutomaticNotifierType.RecentSubscribers,
            m_notifierControllerRecentSubscribers
        );
        m_automaticNotifierControllers.Add(
            AutomaticNotifierType.Twitch,
            m_notifierControllerTwitch
        );
        m_automaticNotifierControllers.Add(
            AutomaticNotifierType.YouTube,
            m_notifierControllerYouTube
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
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );

        twitchManager.ChannelCheered += OnChannelCheered;
        twitchManager.ChannelFollowed += OnChannelFollowed;
        twitchManager.ChannelSubscriptionGifted += OnChannelSubscriptionGifted;
        twitchManager.ChannelSubscribed += OnChannelSubscribed;
    }

    private void StartNotifying()
    {
        m_currentAutomaticNotifierType = (AutomaticNotifierType)m_random.Next(
            (int)AutomaticNotifierType.Discord,
            (int)AutomaticNotifierType.Count
        );
        m_currentNotifierController = m_automaticNotifierControllers[m_currentAutomaticNotifierType];
        m_currentNotifierController.StartNotification();
    }

    private void OnChannelCheered(
        TwitchWebSocketMessagePayloadEventChannelCheer payload
    )
    {
        m_eventNotifierTypeQueue.Enqueue(
            EventNotifierType.NewCheer
        );
    }

    private void OnChannelFollowed(
        TwitchWebSocketMessagePayloadEventChannelFollow payload
    )
    {
        m_eventNotifierTypeQueue.Enqueue(
            EventNotifierType.NewFollower
        );
    }

    private void OnChannelSubscriptionGifted(
        TwitchWebSocketMessagePayloadEventChannelSubscriptionGift payload
    )
    {
        m_eventNotifierTypeQueue.Enqueue(
            EventNotifierType.NewGiftedSubscription
        );
    }

    private void OnChannelSubscribed(
        TwitchWebSocketMessagePayloadEventChannelSubscribe payload
    )
    {
        m_eventNotifierTypeQueue.Enqueue(
            EventNotifierType.NewSubscriber
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

    private void SelectNextAutomaticNotifierController()
    {
        var currentNotifierType = m_currentAutomaticNotifierType;
        while (currentNotifierType == m_currentAutomaticNotifierType)
        {
            m_currentAutomaticNotifierType = (AutomaticNotifierType)m_random.Next(
                (int)AutomaticNotifierType.Discord,
                (int)AutomaticNotifierType.Count
            );
        }

        m_currentNotifierController = m_automaticNotifierControllers[m_currentAutomaticNotifierType];
    }
}