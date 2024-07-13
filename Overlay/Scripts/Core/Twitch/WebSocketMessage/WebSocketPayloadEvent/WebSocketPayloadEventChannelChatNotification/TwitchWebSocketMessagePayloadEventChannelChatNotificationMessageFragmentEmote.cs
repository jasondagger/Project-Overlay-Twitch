
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote
	{
        [JsonPropertyName("emote_set_id")]
        public string EmoteSetId = string.Empty;

        [JsonPropertyName("format")]
        public string[] Format = null;

        [JsonPropertyName("id")]
        public string Id = string.Empty;

        [JsonPropertyName("owner_id")]
        public string OwnerId = string.Empty;

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