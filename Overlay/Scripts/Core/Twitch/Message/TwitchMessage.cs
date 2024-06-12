
namespace Overlay
{
    using System.Text.Json.Serialization;

    public abstract class TwitchMessage
    {
        [JsonPropertyName("type")]
        public TwitchEventSubSubscriptionType Type = TwitchEventSubSubscriptionType.Unknown;

        public TwitchMessage(
            TwitchEventSubSubscriptionType type
        )
        {
            this.Type = type;
        }
    }
}