
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelFollow : TwitchWebSocketMessagePayloadEvent
{
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public string followed_at= string.Empty;
    public string user_id = string.Empty;
    public string user_login = string.Empty;
    public string user_name = string.Empty;
}