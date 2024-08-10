
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewGiftedSubscription : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
			twitchManager.ChannelSubscriptionGifted += OnChannelSubscriptionGifted;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "New Gifted Subscription!";

		private void OnChannelSubscriptionGifted(
			TwitchWebSocketMessagePayloadEventChannelSubscriptionGift payload
		)
		{
			var isChatterAnonymous = payload.IsAnonymous ?? false;
			m_pendingNames.Enqueue(
				item: isChatterAnonymous ? "Anonymous" : payload.UserName
			);
			m_pendingNames.Enqueue(
				item: $"{payload.Total}x Tier {payload.Tier[0]} Gifted Subscription{(payload.Total > 1u ? "s" : "")}!"
			);
		}
	}
}