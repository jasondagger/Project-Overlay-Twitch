
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelSubscribe : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelSubscribe @event { get; set; } = new();
}