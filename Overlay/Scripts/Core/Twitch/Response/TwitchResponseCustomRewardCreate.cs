
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreate
    {
        [JsonPropertyName("data")]
        public TwitchResponseCustomRewardCreateData[] Data = null;
    }
}