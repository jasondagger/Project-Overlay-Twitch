
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubChannelCheer
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        public TwitchConditionEventSubChannelCheer(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}