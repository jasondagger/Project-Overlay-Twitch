
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelSubscriptionGift : TwitchWebSocketMessagePayloadEvent
{
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public int cumulative_total = 0;
    public bool is_anonymous = false;
    public string tier = string.Empty;
    public int total = 0;
    public string user_id = string.Empty;
    public string user_login = string.Empty;
    public string user_name = string.Empty;
}