
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelCheer : TwitchWebSocketMessagePayloadEvent
{
    public int bits = 0;
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public bool is_anonymous = false;
    public string message = string.Empty;
    public string user_id = string.Empty;
    public string user_login = string.Empty;
    public string user_name = string.Empty;
}