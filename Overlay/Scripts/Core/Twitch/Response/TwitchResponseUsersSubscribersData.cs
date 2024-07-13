
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseUsersSubscribersData
    {
        [JsonPropertyName("broadcaster_id")]
        public string BroadcasterId { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_login")]
        public string BroadcasterLogin { get; set; } = string.Empty;

        [JsonPropertyName("broadcaster_name")]
        public string BroadcasterName { get; set; } = string.Empty;

        [JsonPropertyName("gifter_id")]
        public string GifterId { get; set; } = string.Empty;

        [JsonPropertyName("gifter_login")]
        public string GifterLogin { get; set; } = string.Empty;

        [JsonPropertyName("is_gift")]
        public bool IsGift { get; set; } = false;

        [JsonPropertyName("plan_name")]
        public string PlanName { get; set; } = string.Empty;

        [JsonPropertyName("tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = string.Empty;
    }
}