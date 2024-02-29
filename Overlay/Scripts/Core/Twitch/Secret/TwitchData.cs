using Newtonsoft.Json.Linq;
using System.IO;

public sealed class TwitchData
{
    public static string TwitchChannel { get; private set; } = string.Empty;

    public static string AccountUsername { get; private set; } = string.Empty;
    public static string AccountAccessToken { get; private set; } = string.Empty;
    public static string AccountId { get; private set; } = string.Empty;

    public static string BotUsername { get; private set; } = string.Empty;
    public static string BotAccessToken { get; private set; } = string.Empty;

    public static string ClientSecret { get; private set; } = string.Empty;
    public static string ClientId { get; private set; } = string.Empty;

    public static void Load()
    {
        const string path = "Resources\\Twitch\\TwitchData.json";
        string json = File.ReadAllText(
            path    
        );
        JToken jsonParse = JToken.Parse(
            json
        );
        TwitchChannel      = (string)jsonParse["TwitchChannel"];
        AccountUsername    = (string)jsonParse["AccountUsername"];
        AccountAccessToken = (string)jsonParse["AccountAccessToken"];
        AccountId          = (string)jsonParse["AccountId"];
        BotUsername        = (string)jsonParse["BotUsername"];
        BotAccessToken     = (string)jsonParse["BotAccessToken"];
        ClientSecret       = (string)jsonParse["ClientSecret"];
        ClientId           = (string)jsonParse["ClientId"];
    }
}