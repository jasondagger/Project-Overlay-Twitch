
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelRaid : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelRaid @event { get; set; } = new();
}