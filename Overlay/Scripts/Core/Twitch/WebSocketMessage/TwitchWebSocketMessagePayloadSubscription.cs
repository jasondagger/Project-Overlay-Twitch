
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadSubscription
{
    public string id { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string version { get; set; } = string.Empty;
    public string cost { get; set; } = string.Empty;
    public TwitchWebSocketMessageCondition condition { get; set; } = new();
    public TwitchEventSubTransportWebSocket transport { get; set; } = new();
    public string created_at { get; set; } = string.Empty;
}