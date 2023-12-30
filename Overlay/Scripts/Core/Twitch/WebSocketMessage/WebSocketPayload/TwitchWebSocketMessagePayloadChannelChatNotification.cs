
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelChatNotification : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelChatNotification @event { get; set; } = new();
}