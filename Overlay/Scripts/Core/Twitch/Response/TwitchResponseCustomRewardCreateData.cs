
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseCustomRewardCreateData
    {
        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_id")]
        public string BroadcasterId { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_login")]
        public string BroadcasterLogin { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_name")]
        public string BroadcasterName { get; set; } = string.Empty;

        [JsonPropertyName("cooldown_expires_at")]
        public string CooldownExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName("cost")]
        public int Cost { get; set; } = 0;

        [JsonPropertyName("image")]
        public TwitchResponseCustomRewardCreateDataImage Image { get; set; } = null;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("global_cooldown_setting")]
        public TwitchResponseCustomRewardCreateDataGlobalCooldownSetting GlobalCooldownSetting { get; set; } = null;

        [JsonPropertyName("default_image")]
        public TwitchResponseCustomRewardCreateDataImage DefaultImage { get; set; } = null;

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [JsonPropertyName("is_in_stock")]
        public bool IsInStock { get; set; } = false;

        [JsonPropertyName("is_paused")]
        public bool IsPaused { get; set; } = false;

        [JsonPropertyName("is_user_input_required")]
        public bool IsUserInputRequired { get; set; } = false;

        [JsonPropertyName("max_per_stream_setting")]
        public TwitchResponseCustomRewardCreateDataMaxPerStreamSetting MaxPerStreamSetting { get; set; } = null;

        [JsonPropertyName("max_per_user_per_stream_setting")]
        public TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting MaxPerUserPerStreamSetting { get; set; } = null;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("redemptions_redeemed_current_stream")]
        public int? RedemptionsRedeemedCurrentStream { get; set; } = null;

        [JsonPropertyName("should_redemptions_skip_request_queue")]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; } = false;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }
}