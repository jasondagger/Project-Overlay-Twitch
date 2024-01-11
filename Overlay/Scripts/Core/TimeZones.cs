
using System.Linq;

public static class TimeZones
{
    public static bool IsTimeZoneAbbreviationValid(
        string abbreviation    
    )
    {
        return c_timeZones.Contains(
            abbreviation
        );
    }

    public static string[] GetTimeZones()
    {
        return c_timeZones;
    }

    private static readonly string[] c_timeZones = new[]
    {
        "AST",
        "BST",
        "CAT",
        "CST",
        "EAT",
        "EST",
        "GMT",
        "HST",
        "IST",
        "JST",
        "MST",
        "NST",
        "PST",
        "SST",
        "UTC",
    };
}