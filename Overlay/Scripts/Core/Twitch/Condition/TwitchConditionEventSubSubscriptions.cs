
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchConditionEventSubSubscriptions
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId = string.Empty;

        public TwitchConditionEventSubSubscriptions(
            string userId
        )
        {
            BroadcasterUserId = userId;
        }
    }
}