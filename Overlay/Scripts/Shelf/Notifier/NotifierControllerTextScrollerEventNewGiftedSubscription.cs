using NodeType = NodeDirectory.NodeType;

public sealed partial class NotifierControllerTextScrollerEventNewGiftedSubscription : NotifierControllerTextScrollerEvent
{
	public override void _EnterTree()
	{
        var twitchManager = GetNode<TwitchManager>(
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );
        twitchManager.ChannelSubscriptionGifted += OnChannelSubscriptionGifted;

        base._EnterTree();
	}

    protected override string HeaderText { get; set; } = "New Gifted Subscription!";

    private void OnChannelSubscriptionGifted(
        TwitchWebSocketMessagePayloadEventChannelSubscriptionGift payload
    )
    {
        m_pendingNames.Enqueue(
            payload.is_anonymous ? "Anonymous" : payload.user_name
        );
        m_pendingNames.Enqueue(
            $"{payload.total}x Tier {payload.tier[0]} Gifted Subscription{(payload.total > 1u ? "s" : "")}!"
        );
    }
}