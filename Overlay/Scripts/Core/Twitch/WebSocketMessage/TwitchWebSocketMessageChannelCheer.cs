
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelCheer : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelCheer payload { get; set; } = new();
}