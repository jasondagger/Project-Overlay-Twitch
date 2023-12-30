
[System.Serializable]
public sealed class TwitchResponseCustomRewardCreateData
{
    public string background_Color = string.Empty;
    public string broadcaster_id = string.Empty;
    public string broadcaster_login = string.Empty;
    public string broadcaster_name = string.Empty;
    public string cooldown_expires_at = string.Empty;
    public int cost = 0;
    public TwitchResponseCustomRewardCreateDataImage image = null;
    public string id = string.Empty;
    public TwitchResponseCustomRewardCreateDataGlobalCooldownSetting global_cooldown_setting = null;
    public TwitchResponseCustomRewardCreateDataImage default_image = null;
    public bool is_enabled = false;
    public bool is_in_stock = false;
    public bool is_paused = false;
    public bool is_user_input_required = false;
    public TwitchResponseCustomRewardCreateDataMaxPerStreamSetting max_per_stream_setting = null;
    public TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting max_per_user_per_stream_setting = null;
    public string prompt = string.Empty;
    public int? redemptions_redeemed_current_stream = null;
    public bool should_redemptions_skip_request_queue = false;
    public string title = string.Empty;
}