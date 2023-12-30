
public abstract class TwitchMessage
{
    public TwitchEventSubSubscriptionType type = TwitchEventSubSubscriptionType.Unknown;

    public TwitchMessage(
        TwitchEventSubSubscriptionType type
    )
    {
        this.type = type;
    }
}