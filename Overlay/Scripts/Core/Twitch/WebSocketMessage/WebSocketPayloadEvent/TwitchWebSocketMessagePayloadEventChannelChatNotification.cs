
[System.Serializable]
public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotification : TwitchWebSocketMessagePayloadEvent
{
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement announcement = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge[] badges =
        new TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge[4u];
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier bits_badge_tier = new();
    public string broadcaster_user_id = string.Empty;
    public string broadcaster_user_login = string.Empty;
    public string broadcaster_user_name = string.Empty;
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonation charity_donation = new();
    public bool chatter_is_anonymous = false;
    public string chatter_user_id = string.Empty;
    public string chatter_user_login = string.Empty;
    public string chatter_user_name = string.Empty;
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationCommunitySubGift community_sub_gift = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationGiftPaidUpgrade gift_paid_upgrade = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage message = new();
    public string message_id = string.Empty;
    public string notice_type = string.Empty;
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationPayItForward pay_it_forward = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade prime_paid_upgrade = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationRaid raid = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationResub resub = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationSub sub = new();
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationSubGift sub_gift = new();
    public string system_message = string.Empty;
    public TwitchWebSocketMessagePayloadEventChannelChatNotificationUnraid unraid = new();
}