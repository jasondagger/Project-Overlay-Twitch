
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewCheer : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
			twitchManager.ChannelCheered += OnChannelCheered;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "New Cheer!";

		private void OnChannelCheered(
			TwitchWebSocketMessagePayloadEventChannelCheer payload
		)
		{
			m_pendingNames.Enqueue(
				item: payload.IsAnonymous ? "Anonymous" : payload.UserName
			);
			m_pendingNames.Enqueue(
				item: $"{payload.Bits}x Bitt{(payload.Bits > 1u ? "ies" : "y")}!"
			);
		}
	}
}