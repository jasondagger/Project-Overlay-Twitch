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
        var json = File.ReadAllText(
            path: c_twitchDataPath
        );
        var jsonParse = JToken.Parse(
            json: json
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

    private const string c_twitchDataPath = "Resources\\Twitch\\TwitchData.json";
}