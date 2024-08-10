
namespace Overlay
{
    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NodeType = NodeDirectory.NodeType;

    public sealed partial class FireworksController : Node2D
    {
        public override void _Process(
            double delta
        )
        {
            if (m_isFireworksShowActive)
            {
                m_elapsedTimeSinceLastFireworkExplosion -= (float)delta;
                if (m_elapsedTimeSinceLastFireworkExplosion <= 0f)
                {
                    LaunchFireworks();
                    CalculateNextLaunchTimeForFireworks();
                }
            }
        }

        public override void _Ready()
        {
            RetrieveResources();
        }

        private enum FireworkColorType : uint
        {
            Blue = 0u,
            Cyan,
            Green,
            Magenta,
            Red,
            Yellow,
        }

        private const float c_fireworksShowCooldownMax = 1.5f;
        private const float c_fireworksShowCooldownMin = 0.75f;
        private const int c_fireworkDelayedIntervalMaxInMilliseconds = 500;
        private const int c_fireworkDelayedIntervalMinInMilliseconds = 0;
        private const int c_fireworkLifetimeInMilliseconds = 2500;
        private const int c_fireworksColorMax = 1;
        private const int c_fireworksMax = 4;
        private const int c_fireworksMin = 2;
        private const int c_fireworksShowLifetimeInMilliseconds = 10000;
        private const string c_rootPath = "res://Overlay//Scenes/FX//Fireworks";

        private static readonly Dictionary<FireworkColorType, string> c_fireworkTypePrefabPaths = new()
        {
            { FireworkColorType.Blue,    $"{c_rootPath}//Fireworks_Blue.tscn" },
            { FireworkColorType.Cyan,    $"{c_rootPath}//Fireworks_Cyan.tscn" },
            { FireworkColorType.Green,   $"{c_rootPath}//Fireworks_Green.tscn" },
            { FireworkColorType.Magenta, $"{c_rootPath}//Fireworks_Magenta.tscn" },
            { FireworkColorType.Red,     $"{c_rootPath}//Fireworks_Red.tscn" },
            { FireworkColorType.Yellow,  $"{c_rootPath}//Fireworks_Yellow.tscn" },
        };

        private readonly Dictionary<FireworkColorType, PackedScene> m_fireworkPrefabReferences = new();
        private readonly Dictionary<FireworkColorType, int> m_fireworkPrefabCounts = new()
        {
            { FireworkColorType.Blue,    0 },
            { FireworkColorType.Cyan,    0 },
            { FireworkColorType.Green,   0 },
            { FireworkColorType.Magenta, 0 },
            { FireworkColorType.Red,     0 },
            { FireworkColorType.Yellow,  0 },
        };
        private bool m_isFireworksShowActive = false;
        private float m_elapsedTimeSinceLastFireworkExplosion = 0f;

        private void CalculateNextLaunchTimeForFireworks()
        {
            m_elapsedTimeSinceLastFireworkExplosion = (float)GD.RandRange(
                from: c_fireworksShowCooldownMin,
                to: c_fireworksShowCooldownMax
            );
        }

        private void CalculateNumberOfFireworksToLaunch()
        {
            var numberOfFireworks = GD.RandRange(
                from: c_fireworksMin, 
                to: c_fireworksMax
            );

            for (var i = 0; i < numberOfFireworks; i++)
            {
                while (true)
                {
                    var fireworkColorType = (FireworkColorType)GD.RandRange(
                        from: 0,
                        to: (int)FireworkColorType.Yellow
                    );
                    var currentFireworkColorCount = m_fireworkPrefabCounts[fireworkColorType];
                    if (currentFireworkColorCount < c_fireworksColorMax)
                    {
                        m_fireworkPrefabCounts[fireworkColorType]++;
                        break;
                    }
                }
            }
        }

        private void InstantiateFireworks()
        {
            var fireworkColorTypes = Enum.GetValues<FireworkColorType>();
            foreach (var fireworkColorType in fireworkColorTypes)
            {
                var numberOfFireworks = m_fireworkPrefabCounts[fireworkColorType];
                for (var i = 0; i < numberOfFireworks; i++)
                {
                    var fireworkPrefabReference = m_fireworkPrefabReferences[fireworkColorType];
                    var fireworkNode = fireworkPrefabReference.Instantiate();
                    var fireworkNodeParticles = fireworkNode as GpuParticles2D;
                    AddChild(
                        node: fireworkNodeParticles
                    );
                    fireworkNodeParticles.Position = new(
                        x: 0f,
                        y: 0f
                    );
                    fireworkNodeParticles.Emitting = false;

                    _ = Task.Run(
                        action: async () =>
                        {
                            var delay = GD.RandRange(
                                from: c_fireworkDelayedIntervalMinInMilliseconds,
                                to: c_fireworkDelayedIntervalMaxInMilliseconds
                            );

                            await Task.Delay(
                                millisecondsDelay: delay
                            );

                            fireworkNodeParticles.Emitting = true;

                            await Task.Delay(
                                millisecondsDelay: c_fireworkLifetimeInMilliseconds
                            );

                            fireworkNodeParticles.QueueFree();
                        }
                    );
                }

                m_fireworkPrefabCounts[fireworkColorType] = 0;
            }
        }

        private void LaunchFireworks()
        {
            CalculateNumberOfFireworksToLaunch();
            InstantiateFireworks();
        }

        private void OnChannelRaided(
            TwitchWebSocketMessagePayloadEventChannelRaid message
        )
        {
            StartFireworksShow();
        }

        private void RetrieveFireworkPrefabReferences()
        {
            foreach (var fireworkTypePrefabPath in c_fireworkTypePrefabPaths)
            {
                var fireworkColorType = fireworkTypePrefabPath.Key;
                var fireworkPrefabPath = fireworkTypePrefabPath.Value;
                var fireworkPackedScene = ResourceLoader.Load<PackedScene>(
                    path: fireworkPrefabPath
                );
                m_fireworkPrefabReferences.Add(
                    key: fireworkColorType,
                    value: fireworkPackedScene
                );
            }
        }

        private void RetrieveResources()
        {
            RetrieveFireworkPrefabReferences();
            SubscribeToTwitchEvents();
        }

        private void StartFireworksShow()
        {
            _ = Task.Run(
                function: async () =>
                {
                    m_isFireworksShowActive = true;

                    await Task.Delay(
                        millisecondsDelay: c_fireworksShowLifetimeInMilliseconds
                    );

                    m_isFireworksShowActive = false;
                    m_elapsedTimeSinceLastFireworkExplosion = 0f;
                }
            );
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