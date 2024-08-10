
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using UILayoutType = UIManager.UILayoutType;

	[SupportedOSPlatform(platformName: "windows")]
    public abstract partial class UILayoutObject : Node
    {
        public override void _Process(
			double elapsed
		)
		{
			ProcessLayoutUpdate();
		}

        public void UpdateUILayout(
            UILayoutType uiLayoutType
        )
        {
            m_newUILayoutType = uiLayoutType;
        }

        protected void RegisterUILayoutHandler(
            UILayoutType uiLayoutType,
            Action handler
        )
        {
            m_uiLayoutHandlers[key: uiLayoutType] = handler;
        }

        private readonly Dictionary<UILayoutType, Action> m_uiLayoutHandlers = new()
        {
            { UILayoutType.Code,    null },
            { UILayoutType.Default, null },
            { UILayoutType.MTG,     null },
            { UILayoutType.TF2,     null },
        };

        private UILayoutType m_currentUILayoutType = UILayoutType.Default;
        private UILayoutType m_newUILayoutType = UILayoutType.Default;

        private void ProcessLayoutUpdate()
        {
            if (
                m_newUILayoutType.Equals(
                    obj: m_currentUILayoutType
                ) is false
            )
            {
                m_currentUILayoutType = m_newUILayoutType;
                m_uiLayoutHandlers[key: m_currentUILayoutType]?.Invoke();

            }
        }
    }
}