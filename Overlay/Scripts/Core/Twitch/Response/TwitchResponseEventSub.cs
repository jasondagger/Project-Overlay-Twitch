
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchResponseEventSub
    {
        public TwitchEventSubData[] data = null;
        public int total = 0;
        public int total_cost = 0;
        public int max_total_cost = 0;
    }
}