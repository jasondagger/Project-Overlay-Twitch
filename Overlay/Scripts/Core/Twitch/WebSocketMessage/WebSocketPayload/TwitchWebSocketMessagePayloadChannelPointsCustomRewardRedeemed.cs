
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadChannelPointsCustomRewardRedeemed : TwitchWebSocketMessagePayload
{
    public TwitchWebSocketMessagePayloadEventChannelPointsCustomRewardRedeemed @event { get; set; } = new();
}