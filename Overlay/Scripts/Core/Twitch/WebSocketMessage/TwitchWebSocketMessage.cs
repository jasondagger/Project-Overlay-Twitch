
[System.Serializable]
public class TwitchWebSocketMessage
{
    public TwitchWebSocketMessageMetadata metadata { get; set; } = new();
    public TwitchWebSocketMessagePayload payload { get; set; } = new();
}