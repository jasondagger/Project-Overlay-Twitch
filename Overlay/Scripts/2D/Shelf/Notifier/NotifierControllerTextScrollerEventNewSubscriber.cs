
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewSubscriber : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
			twitchManager.ChannelSubscribed += OnChannelSubscribed;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "New Subscriber!";

		private void OnChannelSubscribed(
			TwitchWebSocketMessagePayloadEventChannelSubscribe payload
		)
		{
			m_pendingNames.Enqueue(
				item: payload.UserName
			);
		}
	}
}