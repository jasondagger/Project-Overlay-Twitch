
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotification : TwitchWebSocketMessagePayloadEvent
	{
        [JsonPropertyName("announcement")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationAnnouncement Announcement { get; set; } = new();

        [JsonPropertyName("badges")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge[] Badges { get; set; } =
			new TwitchWebSocketMessagePayloadEventChannelChatNotificationBadge[4u];

        [JsonPropertyName("bits_badge_tier")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationBitsBadgeTier BitsBadgeTier { get; set; } = new();

        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_user_name")]
        public string BroadcasterUsername { get; set; } = string.Empty;

        [JsonPropertyName("charity_donation")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCharityDonation CharityDonation { get; set; } = new();

        [JsonPropertyName("chatter_is_anonymous")]
        public bool? ChatterIsAnonymous { get; set; } = false;

        [JsonPropertyName("chatter_user_id")]
        public string ChatterUserId { get; set; } = string.Empty;

        [JsonPropertyName("chatter_user_login")]
        public string ChatterUserLogin { get; set; } = string.Empty;

        [JsonPropertyName("chatter_user_name")]
        public string ChatterUserName { get; set; } = string.Empty;

        [JsonPropertyName("community_sub_gift")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationCommunitySubGift CommunitySubGift { get; set; } = new();

        [JsonPropertyName("gift_paid_upgrade")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationGiftPaidUpgrade GiftPaidUpgrade { get; set; } = new();

        [JsonPropertyName("message")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessage Message { get; set; } = new();

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("notice_type")]
        public string NoticeType { get; set; } = string.Empty;

        [JsonPropertyName("pay_it_forward")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationPayItForward PayItForward { get; set; } = new();

        [JsonPropertyName("prime_paid_upgrade")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationPrimePaidUpgrade PrimePaidUpgrade { get; set; } = new();

        [JsonPropertyName("raid")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationRaid Raid { get; set; } = new();

        [JsonPropertyName("resub")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationResub Resub { get; set; } = new();

        [JsonPropertyName("sub")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationSub Sub { get; set; } = new();

        [JsonPropertyName("sub_gift")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationSubGift SubGift { get; set; } = new();

        [JsonPropertyName("system_message")]
        public string SystemMessage { get; set; } = string.Empty;

        [JsonPropertyName("unraid")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationUnraid Unraid { get; set; } = new();
	}
}