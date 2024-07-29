
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataGlobalCooldownSetting
    {
        [JsonPropertyName(name: "is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [JsonPropertyName(name: "global_cooldown_seconds")]
        public long GlobalCooldownSeconds { get; set; } = 0;
    }
}