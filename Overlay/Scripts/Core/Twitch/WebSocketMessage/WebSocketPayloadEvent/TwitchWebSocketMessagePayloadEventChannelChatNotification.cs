
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotification : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName(name: "announcement")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement Announcement { get; set; } = null;

        [JsonPropertyName(name: "badges")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge[] Badges { get; set; } = null;

        [JsonPropertyName(name: "bits_badge_tier")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier BitsBadgeTier { get; set; } = null;

        [JsonPropertyName(name: "broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName(name: "charity_donation")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonation CharityDonation { get; set; } = null;

        [JsonPropertyName(name: "chatter_is_anonymous")]
        public bool? ChatterIsAnonymous { get; set; } = false;

        [JsonPropertyName(name: "chatter_user_id")]
        public string ChatterUserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "chatter_user_login")]
        public string ChatterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "chatter_user_name")]
        public string ChatterUserName { get; set; } = string.Empty;

        [JsonPropertyName(name: "community_sub_gift")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCommunitySubGift CommunitySubGift { get; set; } = null;

        [JsonPropertyName(name: "gift_paid_upgrade")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationGiftPaidUpgrade GiftPaidUpgrade { get; set; } = null;

        [JsonPropertyName(name: "message")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage Message { get; set; } = null;

        [JsonPropertyName(name: "message_id")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName(name: "notice_type")]
        public string NoticeType { get; set; } = string.Empty;

        [JsonPropertyName(name: "pay_it_forward")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationPayItForward PayItForward { get; set; } = null;

        [JsonPropertyName(name: "prime_paid_upgrade")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade PrimePaidUpgrade { get; set; } = null;

        [JsonPropertyName(name: "raid")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationRaid Raid { get; set; } = null;

        [JsonPropertyName(name: "resub")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationResub Resub { get; set; } = null;

        [JsonPropertyName(name: "sub")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationSub Sub { get; set; } = null;

        [JsonPropertyName(name: "sub_gift")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationSubGift SubGift { get; set; } = null;

        [JsonPropertyName(name: "system_message")]
        public string SystemMessage { get; set; } = string.Empty;

        [JsonPropertyName(name: "unraid")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationUnraid Unraid { get; set; } = null;
	}
}