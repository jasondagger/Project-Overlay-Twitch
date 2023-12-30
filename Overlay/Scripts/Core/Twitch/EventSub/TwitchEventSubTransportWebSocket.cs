
[System.Serializable]
public sealed class TwitchEventSubTransportWebSocket
{
    public string method { get; set; } = "websocket";
    public string session_id { get; set; } = string.Empty;

    public TwitchEventSubTransportWebSocket()
    {

    }

    public TwitchEventSubTransportWebSocket(
        string session_id
    )
    {
        this.session_id = session_id;
    }
}