
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewCheer : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
				NodeDirectory.NodePaths[NodeType.TwitchManager]
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
				payload.IsAnonymous ? "Anonymous" : payload.Username
			);
			m_pendingNames.Enqueue(
				$"{payload.Bits}x Bitt{(payload.Bits > 1u ? "ies" : "y")}!"
			);
		}
	}
}