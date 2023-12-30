
[System.Serializable]
public sealed class TwitchConditionEventSubChannelRaid
{
    public string to_broadcaster_user_id = string.Empty;

    public TwitchConditionEventSubChannelRaid(
        string userId
    )
    {
        to_broadcaster_user_id = userId;
    }
}