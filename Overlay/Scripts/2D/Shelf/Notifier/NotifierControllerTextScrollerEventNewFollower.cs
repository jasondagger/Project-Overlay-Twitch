
namespace Overlay
{
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerEventNewFollower : NotifierControllerTextScrollerEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
			twitchManager.ChannelFollowed += OnChannelFollowed;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "New Follower!";

		private void OnChannelFollowed(
			TwitchWebSocketMessagePayloadEventChannelFollow payload
		)
		{
			m_pendingNames.Enqueue(
				item: payload.UserName
			);
		}
	}
}