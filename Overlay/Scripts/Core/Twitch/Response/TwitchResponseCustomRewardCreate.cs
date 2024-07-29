
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreate
    {
        [JsonPropertyName(name: "data")]
        public TwitchResponseCustomRewardCreateData[] Data { get; set; } = null;
    }
}