
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelCheer : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelCheer @event { get; set; } = new();
}