
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using NodeType = NodeDirectory.NodeType;

    [SupportedOSPlatform(platformName: "windows")]
    public sealed partial class UIManager : Node
    {
        public enum UILayoutType : uint
        {
            Code = 0u,
            Default,
            MTG,
            TF2,
        }

        public override void _Ready()
        {
            RetrieveResources();
            RegisterLayoutHandlers();
        }

        public void ChangeLayoutType(
            UILayoutType uiLayoutType
        )
        {
            if (
                m_currentUILayoutType.Equals(
                    uiLayoutType
                ) is false
            )
            {
                m_uiLayoutSwapHandlers[key: uiLayoutType].Invoke();
                m_currentUILayoutType = uiLayoutType;
            }
        }

        private readonly Dictionary<UILayoutType, Action> m_uiLayoutSwapHandlers = new()
        {
            { UILayoutType.Code,    null },
            { UILayoutType.Default, null },
            { UILayoutType.MTG,     null },
            { UILayoutType.TF2,     null },
        };
        private UILayoutType m_currentUILayoutType = UILayoutType.Default;
        private ShelfManager m_shelfManager = null;
        private TwitchChatManager m_twitchChatManager = null;

        private void HandleSwapToUILayoutCode()
        {
            m_twitchChatManager.UpdateUILayout(
                uiLayoutType: UILayoutType.Code
            );
            m_shelfManager.UpdateUILayout(
                uiLayoutType: UILayoutType.Code
            );
        }

        private void HandleSwapToUILayoutDefault()
        {
            m_twitchChatManager.UpdateUILayout(
                uiLayoutType: UILayoutType.Default
            );
            m_shelfManager.UpdateUILayout(
                uiLayoutType: UILayoutType.Default
            );
        }

        private void HandleSwapToUILayoutMTG()
        {
            m_twitchChatManager.UpdateUILayout(
                uiLayoutType: UILayoutType.MTG
            );
            m_shelfManager.UpdateUILayout(
                uiLayoutType: UILayoutType.MTG
            );
        }

        private void HandleSwapToUILayoutTF2()
        {
            m_twitchChatManager.UpdateUILayout(
                uiLayoutType: UILayoutType.TF2
            );
            m_shelfManager.UpdateUILayout(
                uiLayoutType: UILayoutType.TF2
            );
        }

        private void RegisterLayoutHandlers()
        {
            m_uiLayoutSwapHandlers[ key: UILayoutType.Code    ] = HandleSwapToUILayoutCode;
            m_uiLayoutSwapHandlers[ key: UILayoutType.Default ] = HandleSwapToUILayoutDefault;
            m_uiLayoutSwapHandlers[ key: UILayoutType.MTG     ] = HandleSwapToUILayoutMTG;
            m_uiLayoutSwapHandlers[ key: UILayoutType.TF2     ] = HandleSwapToUILayoutTF2;
        }

        private void RetrieveResources()
        {
            m_shelfManager = GetNode<ShelfManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.ShelfManager
                )
            );
            m_twitchChatManager = GetNode<TwitchChatManager>(
                path: NodeDirectory.GetNodePath(
                    nodeType: NodeType.TwitchChatManager
                )
            );
        }
    }
}