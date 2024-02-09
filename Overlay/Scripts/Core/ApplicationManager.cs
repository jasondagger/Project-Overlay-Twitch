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
            NodeDirectory.NodePaths[NodeType.InputManager]
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
            $"{nameof(ApplicationManager)}.{nameof(Quit)}() - Application quitting."
        );
#endif

        var root = GetNode<Node>(
            NodeDirectory.NodePaths[NodeType.Root]
        );
        root.PropagateNotification(
            (int)NotificationWMCloseRequest
        );

        const int quitDelayInMilliseconds = 3000;
        await Task.Delay(
            quitDelayInMilliseconds
        );

        var sceneTree = GetTree();
        sceneTree.Quit();
    }

    private void SetWindowProperties()
    {
        ProjectSettings.SetSetting(
            "application/boot_splash/bg_color",
            new Color(0f, 0f, 0f, 0f)
        );
        ProjectSettings.SetSetting(
            "display/window/per_pixel_transparency/allowed",
            true
        );
        ProjectSettings.SetSetting(
            "display/window/per_pixel_transparency/enabled",
            true
        );
        ProjectSettings.SetSetting(
            "display/window/size/borderless",
            true
        );
        ProjectSettings.SetSetting(
            "display/window/size/transparent",
            true
        );
        ProjectSettings.SetSetting(
            "rendering/viewport/transparent_background",
            true
        );

        var nodeTree = GetTree();
        var nodeRoot = nodeTree.Root;
        nodeRoot.Transparent = true;
        nodeRoot.TransparentBg = true;

        var viewport = nodeRoot.GetViewport();
        viewport.TransparentBg = true;

        DisplayServer.WindowSetFlag(
            WindowFlags.Transparent,
            true,
            0
        );
        DisplayServer.WindowSetFlag(
            WindowFlags.MousePassthrough,
            true,
            0
        );
        DisplayServer.WindowSetFlag(
            WindowFlags.Borderless,
            true,
            0
        );
    }
}