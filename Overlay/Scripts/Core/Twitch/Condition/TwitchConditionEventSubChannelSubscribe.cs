
[System.Serializable]
public sealed class TwitchConditionEventSubChannelSubscribe
{
    public string broadcaster_user_id = string.Empty;

    public TwitchConditionEventSubChannelSubscribe(
        string userId
    )
    {
        broadcaster_user_id = userId;
    }
}