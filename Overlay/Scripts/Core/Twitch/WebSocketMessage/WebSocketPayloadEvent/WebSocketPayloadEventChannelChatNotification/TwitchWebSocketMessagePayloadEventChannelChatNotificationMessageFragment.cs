
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment
{
    public enum FragmentType : uint
    {
        Text = 0u,
        Cheermote,
        Emote,
        Mention,
    }

    public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentCheermote cheermote = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote emote = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentMention mention = new();
    public string text = string.Empty;
    public string type = string.Empty;

    public FragmentType GetFragmentType()
    {
        switch (type)
        {
            default:
            case "text":
                return FragmentType.Text;
            case "cheermote":
                return FragmentType.Cheermote;
            case "emote":
                return FragmentType.Emote;
            case "mention":
                return FragmentType.Mention;
        }
    }
}