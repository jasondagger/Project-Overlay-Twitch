
namespace Overlay
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using NodeType = NodeDirectory.NodeType;
    using RequiredFileType = ApplicationManager.RequiredFileType;

    public sealed partial class NotifierControllerTextScrollerRecentEventSubscribers : NotifierControllerTextScrollerRecentEvent
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

        public override void _Ready()
        {
            RetrieveResources();

            base._Ready();
        }

        protected override string HeaderText { get; set; } = "Recent Subscribers!";

		private Queue<string> m_recentSubscriberNames = new();

        private void LoadRecentSubscribers()
        {
			var body = ApplicationManager.ReadRequiredFile(
				requiredFileType: RequiredFileType.RecentSubscribers	
			);
            m_recentSubscriberNames = JsonSerializer.Deserialize<Queue<string>>(
                json: Encoding.UTF8.GetString(
                    bytes: body,
                    index: 0,
                    count: body.Length
                )
            );

            var orderedRecentSubscriberNames = m_recentSubscriberNames.Reverse();
            foreach (var recentSubscriberName in orderedRecentSubscriberNames)
            {
                m_names.Enqueue(
                    item: recentSubscriberName                     
                );
            }
        }

        private void OnChannelSubscribed(
			TwitchWebSocketMessagePayloadEventChannelSubscribe payload
		)
		{
            var username = payload.UserName;
            _ = m_recentSubscriberNames.Dequeue();
            m_recentSubscriberNames.Enqueue(
                item: username
            );
			m_pendingNames.Enqueue(
				item: username
            );
            SaveRecentSubscribers();
		}

        private void RetrieveResources()
		{
            LoadRecentSubscribers();
        }

		private void SaveRecentSubscribers()
		{
			ApplicationManager.WriteRequiredFile(
                requiredFileType: RequiredFileType.RecentSubscribers,
                bytes: Encoding.UTF8.GetBytes(
                    s: JsonSerializer.Serialize(
                        value: m_recentSubscriberNames
                    )
                )
            );
		}
	}
}