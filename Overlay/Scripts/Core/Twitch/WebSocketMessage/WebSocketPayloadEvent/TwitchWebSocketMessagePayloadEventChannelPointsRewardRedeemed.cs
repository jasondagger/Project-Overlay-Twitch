
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed : TwitchWebSocketMessagePayloadEvent
{
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public string id = string.Empty;
    public string redeemed_at = string.Empty;
    public TwitchEventSubReward reward = new();
    public string status = string.Empty;
    public string user_id = string.Empty;
    public string user_input = string.Empty;
    public string user_login = string.Empty;
    public string user_name = string.Empty;
}