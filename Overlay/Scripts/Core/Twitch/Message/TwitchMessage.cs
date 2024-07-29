
namespace Overlay
{
    using System.Text.Json.Serialization;

    public abstract class TwitchMessage
    {
        [JsonPropertyName(name: "type")]
        public TwitchEventSubSubscriptionType Type = TwitchEventSubSubscriptionType.Unknown;

        public TwitchMessage(
            TwitchEventSubSubscriptionType type
        )
        {
            this.Type = type;
        }
    }
}