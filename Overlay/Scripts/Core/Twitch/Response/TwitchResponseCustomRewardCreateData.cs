
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateData
    {
        [JsonPropertyName(name: "background_color")]
        public string BackgroundColor { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_id")]
        public string BroadcasterId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_login")]
        public string BroadcasterLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_name")]
        public string BroadcasterName { get; set; } = string.Empty;

        [JsonPropertyName(name: "cooldown_expires_at")]
        public string CooldownExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName(name: "cost")]
        public int Cost { get; set; } = 0;

        [JsonPropertyName(name: "image")]
        public TwitchResponseCustomRewardCreateDataImage Image { get; set; } = null;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "global_cooldown_setting")]
        public TwitchResponseCustomRewardCreateDataGlobalCooldownSetting GlobalCooldownSetting { get; set; } = null;

        [JsonPropertyName(name: "default_image")]
        public TwitchResponseCustomRewardCreateDataImage DefaultImage { get; set; } = null;

        [JsonPropertyName(name: "is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [JsonPropertyName(name: "is_in_stock")]
        public bool IsInStock { get; set; } = false;

        [JsonPropertyName(name: "is_paused")]
        public bool IsPaused { get; set; } = false;

        [JsonPropertyName(name: "is_user_input_required")]
        public bool IsUserInputRequired { get; set; } = false;

        [JsonPropertyName(name: "max_per_stream_setting")]
        public TwitchResponseCustomRewardCreateDataMaxPerStreamSetting MaxPerStreamSetting { get; set; } = null;

        [JsonPropertyName(name: "max_per_user_per_stream_setting")]
        public TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting MaxPerUserPerStreamSetting { get; set; } = null;

        [JsonPropertyName(name: "prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName(name: "redemptions_redeemed_current_stream")]
        public int? RedemptionsRedeemedCurrentStream { get; set; } = null;

        [JsonPropertyName(name: "should_redemptions_skip_request_queue")]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; } = false;

        [JsonPropertyName(name: "title")]
        public string Title { get; set; } = string.Empty;
    }
}