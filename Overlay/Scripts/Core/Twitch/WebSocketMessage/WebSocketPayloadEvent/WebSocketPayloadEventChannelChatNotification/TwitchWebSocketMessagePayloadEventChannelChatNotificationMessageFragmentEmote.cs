
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote
{
    public string emote_set_id = string.Empty;
    public string[] format = null;
    public string id = string.Empty;
    public string owner_id = string.Empty;

    public bool HasAnimation()
    {
        foreach (var word in format)
        {
            if (word == "animated")
            {
                return true;
            }
        }
        return false;
    }
}