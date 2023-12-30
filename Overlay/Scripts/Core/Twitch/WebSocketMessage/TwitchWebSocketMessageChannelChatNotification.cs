
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelChatNotification : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelChatNotification payload { get; set; } = new();
}