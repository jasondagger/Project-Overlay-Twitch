
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
				NodeDirectory.NodePaths[NodeType.TwitchManager]
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
			var index = 0;
			while (m_names.Count < c_maxNameCount)
			{
				var name = response[index++].Username;
				if (name == TwitchData.AccountUsername)
				{
					continue;
				}
				else if (
					name.All(
						predicate: char.IsAscii
					) is false
				)
				{
					name = response[index].UserLogin;
				}

				m_names.Enqueue(
					item: name
				);
			}
		}
	}
}