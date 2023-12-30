
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelSubscriptionGift : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelSubscriptionGift @event { get; set; } = new();
}