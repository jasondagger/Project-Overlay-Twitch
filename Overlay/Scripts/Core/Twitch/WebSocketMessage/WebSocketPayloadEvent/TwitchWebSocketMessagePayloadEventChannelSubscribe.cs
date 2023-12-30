
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelSubscribe : TwitchWebSocketMessagePayloadEvent
{
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public bool is_gift = false;
    public string tier = string.Empty;
    public string user_id = string.Empty;
    public string user_login = string.Empty;
    public string user_name = string.Empty;
}