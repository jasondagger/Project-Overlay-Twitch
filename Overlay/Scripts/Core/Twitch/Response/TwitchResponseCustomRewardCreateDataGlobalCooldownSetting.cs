
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataGlobalCooldownSetting
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [JsonPropertyName("global_cooldown_seconds")]
        public long GlobalCooldownSeconds { get; set; } = 0;
    }
}