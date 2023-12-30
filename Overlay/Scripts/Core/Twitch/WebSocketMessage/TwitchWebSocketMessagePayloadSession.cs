
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadSession
{
    public string id { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public int keepalive_timeout_seconds { get; set; } = 0;
    public string reconnect_url { get; set; } = string.Empty;
    public string connected_at { get; set; } = string.Empty;
}