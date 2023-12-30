
[System.Serializable]
public sealed class TwitchConditionEventSubChannelCheer
{
    public string broadcaster_user_id = string.Empty;

    public TwitchConditionEventSubChannelCheer()
    {

    }

    public TwitchConditionEventSubChannelCheer(
        string userId
    )
    {
        broadcaster_user_id = userId;
    }
}