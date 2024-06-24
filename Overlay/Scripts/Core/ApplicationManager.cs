
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Environment = System.Environment;
    using KeyBindType = InputManager.KeyBindType;
    using NodeType = NodeDirectory.NodeType;
    using WindowFlags = Godot.DisplayServer.WindowFlags;

    public sealed partial class ApplicationManager : Node
    {
        public override void _EnterTree()
        {
            SetWindowProperties();
            CreateDirectories();
            BindInputEvents();
        }

        public static void CreateAnimatedEmoteDirectory(
            string emoteName
        )
        {
            var userDirectoryPath = $"{c_userDirectoryPaths[UserDirectoryType.AnimatedEmotes]}\\{emoteName}";
            if (
                Directory.Exists(
                    path: userDirectoryPath
                ) is false
            )
            {
                var relativePath = $"{c_relativeDirectoryPaths[UserDirectoryType.AnimatedEmotes]}\\{emoteName}";
                var fullPath = GetFullPathForRelativeUserDirectory(
                    relativePath: relativePath
                );
                _ = Directory.CreateDirectory(
                    path: fullPath
                );
            }
        }

        public static void CreateStaticEmoteDirectory(
            string emoteName
        )
        {
            var userDirectoryPath = $"{c_userDirectoryPaths[UserDirectoryType.StaticEmotes]}\\{emoteName}";
            if (
                Directory.Exists(
                    path: userDirectoryPath
                ) is false
            )
            {
                var relativePath = $"{c_relativeDirectoryPaths[UserDirectoryType.StaticEmotes]}\\{emoteName}";
                var fullPath = GetFullPathForRelativeUserDirectory(
                    relativePath: relativePath
                );
                _ = Directory.CreateDirectory(
                    path: fullPath
                );
            }
        }

        public static string GetAnimatedEmoteDirectory(
            string emoteName    
        )
        {
            var relativePath = $"{c_relativeDirectoryPaths[UserDirectoryType.AnimatedEmotes]}/{emoteName}";
            return GetFullPathForRelativeUserDirectory(
                relativePath: relativePath
            );
        }

        public static string GetFullPathForRelativeUserDirectory(
            string relativePath
        )
        {
            return Environment.ExpandEnvironmentVariables(
                name: $"{c_applicationEnvironmentPath}\\{relativePath}"
            );
        }

        public static string GetStaticEmoteDirectory(
            string emoteName    
        )
        {
            var relativePath = $"{c_relativeDirectoryPaths[UserDirectoryType.StaticEmotes]}/{emoteName}";
            return GetFullPathForRelativeUserDirectory(
                relativePath: relativePath
            );
        }

        private enum UserDirectoryType : uint
        {
            AnimatedEmotes = 0u,
            Badges,
            StaticEmotes,
        }

        private const string c_applicationEnvironmentPath = "%APPDATA%\\Godot\\app_userdata";

        private static readonly Dictionary<UserDirectoryType, string> c_userDirectoryPaths = new()
        {
            { UserDirectoryType.Badges,         "users://Badges" },
            { UserDirectoryType.AnimatedEmotes, "users://Emotes/Animated" },
            { UserDirectoryType.StaticEmotes,   "users://Emotes/Static" },
        };
        private static readonly Dictionary<UserDirectoryType, string> c_relativeDirectoryPaths = new()
        {
            { UserDirectoryType.Badges,         "Overlay/Badges" },
            { UserDirectoryType.AnimatedEmotes, "Overlay/Emotes/Animated" },
            { UserDirectoryType.StaticEmotes,   "Overlay/Emotes/Static" },
        };

        private void BindInputEvents()
        {
            var inputManager = GetNode<InputManager>(
                path: NodeDirectory.NodePaths[NodeType.InputManager]
            );
            inputManager.KeyBindPressed[KeyBindType.ApplicationManagerQuit] += OnPressedApplicationQuit;
        }

        private static void CreateDirectories()
        {
            var userDirectoryTypes = Enum.GetValues<UserDirectoryType>();
            foreach (var userDirectoryType in userDirectoryTypes)
            {
                var userDirectoryPath = c_userDirectoryPaths[userDirectoryType];
                if (
                    Directory.Exists(
                        path: userDirectoryPath
                    ) is false
                )
                {
                    var relativePath = c_relativeDirectoryPaths[userDirectoryType];
                    var fullPath = GetFullPathForRelativeUserDirectory(
                        relativePath: relativePath
                    );
                    _ = Directory.CreateDirectory(
                        path: fullPath
                    );
                }
            }
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