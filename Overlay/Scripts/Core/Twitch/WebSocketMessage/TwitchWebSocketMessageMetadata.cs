
[System.Serializable]
public sealed class TwitchWebSocketMessageMetadata
{
    public string message_id { get; set; } = string.Empty;
    public string message_type { get; set; } = string.Empty;
    public string message_timestamp { get; set; } = string.Empty;
    public string subscription_type { get; set; } = string.Empty;
    public string subscription_version { get; set; } = string.Empty;
}