
[System.Serializable]
public sealed class TwitchConditionEventSubChannelChatNotification
{
    public string broadcaster_user_id = string.Empty;
    public string user_id = string.Empty;

    public TwitchConditionEventSubChannelChatNotification()
    {

    }

    public TwitchConditionEventSubChannelChatNotification(
        string userId
    )
    {
        broadcaster_user_id = userId;
        user_id = userId;
    }
}