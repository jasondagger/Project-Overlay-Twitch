
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelRaid : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelRaid payload { get; set; } = new();
}