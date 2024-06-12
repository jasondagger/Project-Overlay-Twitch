
namespace Overlay
{
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
				payload.Username
			);
		}

		private void OnRecentFollowersRetrieved(
			TwitchResponseChannelFollowersData[] response
		)
		{
			uint index = 0u;
			while (m_names.Count < c_maxNameCount)
			{
				string name = response[index++].Username;
				if (name == TwitchData.AccountUsername)
				{
					continue;
				}
				else if (
					!name.All(
						char.IsAscii
					)
				)
				{
					name = response[index].UserLogin;
				}

				m_names.Enqueue(
					name
				);
			}
		}
	}
}