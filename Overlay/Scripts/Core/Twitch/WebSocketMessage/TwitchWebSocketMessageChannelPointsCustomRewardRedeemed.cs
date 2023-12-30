
[System.Serializable]
public sealed class TwitchWebSocketMessageChannelPointsCustomRewardRedeemed : TwitchWebSocketMessage
{
    public new TwitchWebSocketMessagePayloadChannelPointsCustomRewardRedeemed payload { get; set; } = new();
}