
namespace Overlay
{
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
            ShelfManager,
            SpotifyManager,
            TwitchBot,
            TwitchChannelPointRewardsManager,
            TwitchChatManager,
            TwitchManager,
            UIManager,
        }

        public static string GetNodePath(
            NodeType nodeType    
        )
        {
            return c_nodePaths[key: nodeType];
        }

        private static readonly Dictionary<NodeType, string> c_nodePaths = new()
        {
            { NodeType.ApplicationManager,               $"{c_core}/{nameof(NodeType.ApplicationManager)}" },
            { NodeType.AudioManager,                     $"{c_core}/{nameof(NodeType.AudioManager)}" },
            { NodeType.HttpManager,                      $"{c_core}/{nameof(NodeType.HttpManager)}" },
            { NodeType.InputManager,                     $"{c_core}/{nameof(NodeType.InputManager)}" },
            { NodeType.PastelInterpolator,               $"{c_core}/{nameof(NodeType.PastelInterpolator)}" },
            { NodeType.Root,                             $"{c_root}" },
            { NodeType.ShelfManager,                     $"{c_2d}/{nameof(NodeType.ShelfManager)}" },
            { NodeType.SpotifyManager,                   $"{c_core}/{nameof(NodeType.SpotifyManager)}" },
            { NodeType.TwitchBot,                        $"{c_core}/{nameof(NodeType.TwitchBot)}" },
            { NodeType.TwitchChannelPointRewardsManager, $"{c_core}/{nameof(NodeType.TwitchChannelPointRewardsManager)}" },
            { NodeType.TwitchChatManager,                $"{c_2d}/{nameof(NodeType.TwitchChatManager)}" },
            { NodeType.TwitchManager,                    $"{c_core}/{nameof(NodeType.TwitchManager)}" },
            { NodeType.UIManager,                        $"{c_2d}/{nameof(NodeType.UIManager)}" },
        };

        private const string c_root = "/root";
        private const string c_main = $"{c_root}/Main";
        private const string c_core = $"{c_main}/Core";
        private const string c_2d = $"{c_main}/2D";
    }
}