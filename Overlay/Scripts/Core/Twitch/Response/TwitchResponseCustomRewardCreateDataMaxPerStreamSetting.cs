
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataMaxPerStreamSetting
    {
        [JsonPropertyName(name: "is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [JsonPropertyName(name: "max_per_stream")]
        public long MaxPerStream { get; set; } = 0;
    }
}