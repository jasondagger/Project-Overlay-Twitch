
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubChannelSubscribe
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelSubscribe(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}