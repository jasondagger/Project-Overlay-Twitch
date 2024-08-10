
namespace Overlay
{
    using Godot;
    using Godot.Collections;
    using System;

    public sealed partial class TwitchEventManager : Node
    {
        public override void _Ready()
        {

        }

        public override void _Process(
            double delta
        )
        {

        }

        private enum TwitchEventNotificationType : uint
        {
            ChannelBits = 0u,
            Follower,
            Raid,
            Subscriber,
        }

        private readonly Dictionary<TwitchEventNotificationType, string> c_temp = new();
    }
}