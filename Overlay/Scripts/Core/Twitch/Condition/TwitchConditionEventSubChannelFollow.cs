
[System.Serializable]
public sealed class TwitchConditionEventSubChannelFollow
{
    public string broadcaster_user_id = string.Empty;
    public string moderator_user_id = string.Empty;

    public TwitchConditionEventSubChannelFollow(
        string userId
    )
    {
        broadcaster_user_id = userId;
        moderator_user_id = userId;
    }
}