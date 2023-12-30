using Godot;
using System.Threading.Tasks;
using KeyBindType = InputManager.KeyBindType;
using NodeType = NodeDirectory.NodeType;

public sealed partial class ApplicationManager : Node
{
	public override void _EnterTree()
	{
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
}