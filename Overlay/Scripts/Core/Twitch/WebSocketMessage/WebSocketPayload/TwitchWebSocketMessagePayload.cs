
[System.Serializable]
public class TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadSession session { get; set; } = new();
    public TwitchWebSocketMessagePayloadSubscription subscription { get; set; } = new();
}