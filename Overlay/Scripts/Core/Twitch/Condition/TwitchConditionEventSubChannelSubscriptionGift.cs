
[System.Serializable]
public sealed class TwitchConditionEventSubChannelSubscriptionGift
{
    public string broadcaster_user_id = string.Empty;

    public TwitchConditionEventSubChannelSubscriptionGift(
        string userId
    )
    {
        broadcaster_user_id = userId;
    }
}