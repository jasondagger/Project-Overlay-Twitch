
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote
	{
        [JsonPropertyName(name: "emote_set_id")]
        public string EmoteSetId { get; set; } = string.Empty;

        [JsonPropertyName(name: "format")]
        public string[] Format { get; set; } = null;

        [JsonPropertyName(name: "id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName(name: "owner_id")]
        public string OwnerId { get; set; } = string.Empty;

		public bool HasAnimation()
		{
			foreach (var word in Format)
			{
				if (word is "animated")
				{
					return true;
				}
			}
			return false;
		}
	}
}