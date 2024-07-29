
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersSubscribersData
    {
        [JsonPropertyName(name: "broadcaster_id")]
        public string BroadcasterId { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_login")]
        public string BroadcasterLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "broadcaster_name")]
        public string BroadcasterName { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_id")]
        public string GifterId { get; set; } = string.Empty;

        [JsonPropertyName(name: "gifter_login")]
        public string GifterLogin { get; set; } = string.Empty;

        [JsonPropertyName(name: "is_gift")]
        public bool IsGift { get; set; } = false;

        [JsonPropertyName(name: "plan_name")]
        public string PlanName { get; set; } = string.Empty;

        [JsonPropertyName(name: "tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_name")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName(name: "user_login")]
        public string UserLogin { get; set; } = string.Empty;
    }
}