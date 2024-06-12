
namespace Overlay
{
    using Godot;
    using System.Threading.Tasks;
    using KeyBindType = InputManager.KeyBindType;
    using NodeType = NodeDirectory.NodeType;
    using WindowFlags = Godot.DisplayServer.WindowFlags;

    public sealed partial class ApplicationManager : Node
    {
        public override void _EnterTree()
        {
            SetWindowProperties();
            BindInputEvents();
        }

        private void BindInputEvents()
        {
            var inputManager = GetNode<InputManager>(
                path: NodeDirectory.NodePaths[NodeType.InputManager]
            );
            inputManager.KeyBindPressed[KeyBindType.ApplicationManagerQuit] += OnPressedApplicationQuit;
        }

        private void OnPressedApplicationQuit()
        {
            Quit();
        }

        private async void Quit()
        {
#if DEBUG
            GD.Print(
                what: $"{nameof(ApplicationManager)}.{nameof(Quit)}() - Application quitting."
            );
#endif

            var root = GetNode<Node>(
                path: NodeDirectory.NodePaths[NodeType.Root]
            );
            root.PropagateNotification(
                what: (int)NotificationWMCloseRequest
            );

            const int quitDelayInMilliseconds = 3000;
            await Task.Delay(
                millisecondsDelay: quitDelayInMilliseconds
            );

            var sceneTree = GetTree();
            sceneTree.Quit();
        }

        private void SetWindowProperties()
        {
            ProjectSettings.SetSetting(
                name: "application/boot_splash/bg_color",
                value: new Color(0f, 0f, 0f, 0f)
            );
            ProjectSettings.SetSetting(
                name: "display/window/per_pixel_transparency/allowed",
                value: true
            );
            ProjectSettings.SetSetting(
                name: "display/window/per_pixel_transparency/enabled",
                value: true
            );
            ProjectSettings.SetSetting(
                name: "display/window/size/borderless",
                value: true
            );
            ProjectSettings.SetSetting(
                name: "display/window/size/transparent",
                value: true
            );
            ProjectSettings.SetSetting(
                name: "rendering/viewport/transparent_background",
                value: true
            );

            var nodeTree = GetTree();
            var nodeRoot = nodeTree.Root;
            nodeRoot.Transparent = true;
            nodeRoot.TransparentBg = true;

            var viewport = nodeRoot.GetViewport();
            viewport.TransparentBg = true;

            DisplayServer.WindowSetFlag(
                flag: WindowFlags.Transparent,
                enabled: true,
                windowId: 0
            );
            DisplayServer.WindowSetFlag(
                flag: WindowFlags.MousePassthrough,
                enabled: true,
                windowId: 0
            );
            DisplayServer.WindowSetFlag(
                flag: WindowFlags.Borderless,
                enabled: true,
                windowId: 0
            );
        }
    }
}