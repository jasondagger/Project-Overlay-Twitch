using NodeType = NodeDirectory.NodeType;

public sealed partial class NotifierControllerTextScrollerRecentEventSubscribers : NotifierControllerTextScrollerRecentEvent
{
	public override void _EnterTree()
	{
		var twitchManager = GetNode<TwitchManager>(
            NodeDirectory.NodePaths[NodeType.TwitchManager]
        );

        twitchManager.ChannelSubscribed += OnChannelSubscribed;
        twitchManager.GiftedSubscribersRetrieved += OnGiftedSubscribersRetrieved;
        twitchManager.SubscribersRetrieved += OnSubscribersRetrieved;

        base._EnterTree();
    }

    protected override string HeaderText { get; set; } = "Recent Subscribers!";

    private const string c_recentSubscribersText = "Resources\\Twitch\\RecentSubscribers.txt";

    private void OnChannelSubscribed(
        TwitchWebSocketMessagePayloadEventChannelSubscribe payload
    )
    {
        m_pendingNames.Enqueue(
            payload.user_name
        );
    }

    private void OnSubscribersRetrieved(
        TwitchResponseUsersSubscribersData[] response
    )
    {
        //string[] recentSubscriberNames = File.ReadAllLines(
        //    c_recentSubscribersText
        //);
        int index = response.Length - 1;
        while (m_names.Count < c_maxNameCount && index >= 0u)
        {
            string name = response[index--].user_name;
            if (name.ToLower() == TwitchData.AccountUsername.ToLower())
            {
                continue;
            }

            m_names.Enqueue(
                name
            );
        }
    }

    private void OnGiftedSubscribersRetrieved(
        TwitchResponseUsersSubscribersData[] response
    )
    {
        int index = response.Length - 1;
        while (index >= 0u)
        {
            string name = response[index--].user_name;
            if (name.ToLower() == TwitchData.AccountUsername.ToLower())
            {
                continue;
            }

            m_pendingNames.Enqueue(
                name
            );
        }
    }
}