
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelFollow : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelFollow @event { get; set; } = new();
}