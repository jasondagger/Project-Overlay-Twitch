
namespace Overlay
{
    using System.Runtime.Versioning;

	[SupportedOSPlatform(platformName: "windows")]
    public sealed partial class ShelfManager : UILayoutObject
    {
        public override void _Ready()
        {
            base._Ready();

            RetrieveResources();
        }

        protected override void HandleSwapToUILayoutToCode()
        {
            m_shelfImageAudioWave.Visible = false;
            m_shelfImageBackground.Visible = false;
            m_nameplateController.Visible = false;
            m_notifierController.Visible = false;
        }

        protected override void HandleSwapToUILayoutToDefault()
        {
            m_shelfImageAudioWave.Visible = false;
            m_shelfImageBackground.Visible = false;
            m_nameplateController.Visible = false;
            m_notifierController.Visible = false;
        }

        protected override void HandleSwapToUILayoutToMTG()
        {
            m_shelfImageAudioWave.Visible = false;
            m_shelfImageBackground.Visible = false;
            m_nameplateController.Visible = false;
            m_notifierController.Visible = false;
        }

        protected override void HandleSwapToUILayoutToTF2()
        {
            m_shelfImageAudioWave.Visible = true;
            m_shelfImageBackground.Visible = true;
            m_nameplateController.Visible = true;
            m_notifierController.Visible = true;
        }

        private NameplateController m_nameplateController = null;
        private NotifierController m_notifierController = null;
        private ShelfImageAudioWave m_shelfImageAudioWave = null;
        private ShelfImageBackground m_shelfImageBackground = null;

        private void RetrieveResources()
        {
            m_nameplateController = GetNode<NameplateController>(
                path: "NameplateController"
            );
            m_notifierController = GetNode<NotifierController>(
                path: "NotifierController"
            );
            m_shelfImageAudioWave = GetNode<ShelfImageAudioWave>(
                path: "ShelfImageAudioWave"
            );
            m_shelfImageBackground = GetNode<ShelfImageBackground>(
                path: "ShelfImageBackground"
            );
        }
    }
}