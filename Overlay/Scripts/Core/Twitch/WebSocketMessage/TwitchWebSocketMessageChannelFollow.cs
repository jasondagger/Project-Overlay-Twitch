
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelFollow : TwitchWebSocketMessage 
{
    public new TwitchWebSocketMessagePayloadChannelFollow payload { get; set; } = new();
}