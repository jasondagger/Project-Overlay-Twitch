using System.Collections.Generic;

public sealed class NodeDirectory
{
    public enum NodeType : uint
    {
        ApplicationManager = 0u,
        AudioManager,
        HttpManager,
        InputManager,
        PastelInterpolator,
        Root,
        TwitchBot,
        TwitchChannelPointRewardsManager,
        TwitchChatManager,
        TwitchManager
    }

    public static readonly Dictionary<NodeType, string> NodePaths = new()
    {
        { NodeType.ApplicationManager,               $"{c_core}/ApplicationManager" },
        { NodeType.AudioManager,                     $"{c_core}/AudioManager" },
        { NodeType.HttpManager,                      $"{c_core}/HttpManager" },
        { NodeType.InputManager,                     $"{c_core}/InputManager" },
        { NodeType.PastelInterpolator,               $"{c_core}/PastelInterpolator" },
        { NodeType.Root,                             $"{c_root}" },
        { NodeType.TwitchBot,                        $"{c_core}/TwitchBot" },
        { NodeType.TwitchChannelPointRewardsManager, $"{c_core}/TwitchChannelPointRewardsManager" },
        { NodeType.TwitchChatManager,                $"{c_2d}/TwitchChatManager" },
        { NodeType.TwitchManager,                    $"{c_core}/TwitchManager" },
    };

    private const string c_root = "/root";
    private const string c_main = $"{c_root}/Main";
    private const string c_core = $"{c_main}/Core";
    private const string c_2d = $"{c_main}/2D";
}