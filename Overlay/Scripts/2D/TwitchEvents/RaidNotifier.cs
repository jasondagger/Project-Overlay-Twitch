
namespace Overlay
{
    using Godot;
    using NodeType = NodeDirectory.NodeType;

    public sealed partial class RaidNotifier : Node
    {
        public override void _Process(
            double delta
        )
        {

        }

        public override void _Ready()
        {
            //RetrieveResources();
        }

		private const string c_labelColor = $"00000000";
        private const string c_labelFont = $"[font=res://Overlay/Fonts/Roboto-Black.ttf]";
        private const string c_labelFontSize = $"[font_size=68]";
        private const string c_labelOutlineColor = $"[outline_color=#202020FF]";
        private const string c_labelOutlineSize = $"[outline_size=2]";
        private const string c_labelWave = $"[wave amp=50]";
        private const string c_nodePathRelativeViewportContainer = "ViewportContainer/Viewport";

        private const float c_raidNotifierLifetimeInSeconds = 10f;

        private PastelInterpolator m_pastelInterpolator = null;
        private RichTextLabel m_textLabel = null;
        private bool m_isNotifying = false;
        private float m_elapsed = 0f;
        private string m_text = string.Empty;

        private void OnChannelRaided(
            TwitchWebSocketMessagePayloadEventChannelRaid message
        )
        {
            var username = message.FromBroadcasterUserName;
            var lowercaseUsername = username.ToLower();
            var viewerCount = message.Viewers;

            m_text =
                $"{c_labelFont}" +
                $"{c_labelFontSize}" +
                $"{c_labelOutlineColor}" +
                $"{c_labelOutlineSize}" +
                $"[color=#{c_labelColor}]" +
                $"{c_labelWave}" +
                $"{lowercaseUsername}" +
                $"[/wave]" +
                $"[/color]" +
                $" is " +
                $"[color=#{c_labelColor}]" +
                $"{c_labelWave}" +
                $"RAIDING" +
                $"[/wave]" +
                $"[/color]" +
                $"\nwith " +
                $"[color=#{c_labelColor}]" +
                $"{c_labelWave}" +
                $"{viewerCount} VIWERS" +
                $"[/wave]" +
                $"[/color]" +
                $"!";
        }

        private void RetrieveResources()
        {
            m_textLabel = GetNode<RichTextLabel>(
                path: $"{c_nodePathRelativeViewportContainer}/Main"
            );
            m_pastelInterpolator = GetNode<PastelInterpolator>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.PastelInterpolator
                )
            );
            SubscribeToTwitchEvents();
        }

        private void SubscribeToTwitchEvents()
        {
            var twitchManager = GetNode<TwitchManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchManager
                )
            );
            twitchManager.ChannelRaided += OnChannelRaided;
        }
    }
}