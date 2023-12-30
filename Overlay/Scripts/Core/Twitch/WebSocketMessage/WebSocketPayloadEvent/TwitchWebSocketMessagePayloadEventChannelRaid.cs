
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelRaid : TwitchWebSocketMessagePayloadEvent
{
    public string from_broadcaster_user_id = string.Empty;
    public string from_broadcaster_user_login = string.Empty;
    public string from_broadcaster_user_name = string.Empty;
    public string to_broadcaster_user_id = string.Empty;
    public string to_broadcaster_user_login = string.Empty;
    public string to_broadcaster_user_name = string.Empty;
    public int viewers = 0;
}