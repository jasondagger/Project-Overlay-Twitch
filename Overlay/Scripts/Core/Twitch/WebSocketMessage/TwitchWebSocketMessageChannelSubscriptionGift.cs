
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelSubscriptionGift : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelSubscriptionGift payload { get; set; } = new();
}