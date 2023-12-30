
[System.Serializable]
public sealed class TwitchConditionEventSubSubscriptions
{
    public string broadcaster_user_id = string.Empty;

    public TwitchConditionEventSubSubscriptions(
        string userId
    )
    {
        broadcaster_user_id = userId;
    }
}