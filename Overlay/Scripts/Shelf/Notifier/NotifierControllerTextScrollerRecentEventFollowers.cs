
namespace Overlay
{
    using System.Collections.Generic;
    using System.Linq;
	using NodeType = NodeDirectory.NodeType;

	public sealed partial class NotifierControllerTextScrollerRecentEventFollowers : NotifierControllerTextScrollerRecentEvent
	{
		public override void _EnterTree()
		{
			var twitchManager = GetNode<TwitchManager>(
				path: NodeDirectory.NodePaths[NodeType.TwitchManager]
			);

			twitchManager.ChannelFollowed += OnChannelFollowed;
			twitchManager.FollowersRetrieved += OnRecentFollowersRetrieved;

			base._EnterTree();
		}

		protected override string HeaderText { get; set; } = "Recent Followers!";

		private void OnChannelFollowed(
			TwitchWebSocketMessagePayloadEventChannelFollow payload
		)
		{
			m_pendingNames.Enqueue(
				item: payload.Username
			);
		}

		private void OnRecentFollowersRetrieved(
			List<TwitchResponseChannelFollowersData> response
		)
		{
			var recentFollowers = response.Take(
				count: (int)c_maxNameCount
            ).Reverse();
            foreach (var recentFollower in recentFollowers)
            {
				var followerName = recentFollower.Username;
                m_names.Enqueue(
					item: followerName
                );
            }
		}
	}
}