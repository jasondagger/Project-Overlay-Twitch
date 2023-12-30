
[System.Serializable]
public sealed class TwitchResponseEventSubSubscriptionsData
{
    public string id;
    public string status;
    public string type;
    public string version;
    public TwitchConditionEventSubSubscriptions condition;
    public string created_at;
    public TwitchEventSubTransportWebSocket transport;
    public int cost;
}