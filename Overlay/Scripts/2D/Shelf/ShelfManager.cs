
namespace Overlay
{
    using Godot;
    using System.Runtime.Versioning;
    using UILayoutType = UIManager.UILayoutType;

	[SupportedOSPlatform(platformName: "windows")]
    public sealed partial class ShelfManager : UILayoutObject
    {
        public override void _Ready()
        {
            RetrieveResources();
            RegisterLayoutHandlers();
        }

        private void RegisterLayoutHandlers()
        {
            RegisterUILayoutHandler(
                uiLayoutType: UILayoutType.Code,
                handler: SetUILayoutToDevelopment
            );
            RegisterUILayoutHandler(
                uiLayoutType: UILayoutType.TF2,
                handler: SetUILayoutToGame
            );
        }

        private ShelfImageAudioWave m_shelfImageAudioWave = null;
        private ShelfImageBackground m_shelfImageBackground = null;
        private NameplateController m_nameplateController = null;
        private NotifierController m_notifierController = null;

        private void RetrieveResources()
        {
            m_shelfImageAudioWave = GetNode<ShelfImageAudioWave>(
                path: "ShelfImageAudioWave"    
            );
            m_shelfImageBackground = GetNode<ShelfImageBackground>(
                path: "ShelfImageBackground"    
            );
            m_nameplateController = GetNode<NameplateController>(
                path: "NameplateController"    
            );
            m_notifierController = GetNode<NotifierController>(
                path: "NotifierController"    
            );
        }

        private void SetUILayoutToDevelopment()
        {
            m_shelfImageAudioWave.Visible = false;
            m_shelfImageBackground.Visible = false;
            m_nameplateController.Visible = false;
            m_notifierController.Visible = false;
        }

        private void SetUILayoutToGame()
        {
            m_shelfImageAudioWave.Visible = true;
            m_shelfImageBackground.Visible = true;
            m_nameplateController.Visible = true;
            m_notifierController.Visible = true;
        }
    }
}