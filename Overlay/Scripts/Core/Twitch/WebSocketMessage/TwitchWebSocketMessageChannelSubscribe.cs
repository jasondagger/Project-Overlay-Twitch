
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelSubscribe : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelSubscribe payload { get; set; } = new();
}