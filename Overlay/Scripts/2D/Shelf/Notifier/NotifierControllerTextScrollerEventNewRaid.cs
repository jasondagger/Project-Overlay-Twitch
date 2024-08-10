
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewRaid : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
			);
			twitchManager.ChannelRaided += OnChannelRaided;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "Raid HYPE!";

		private void OnChannelRaided(
			TwitchWebSocketMessagePayloadEventChannelRaid payload
		)
		{
			m_pendingNames.Enqueue(
				payload.FromBroadcasterUserName
			);
		}
	}
}