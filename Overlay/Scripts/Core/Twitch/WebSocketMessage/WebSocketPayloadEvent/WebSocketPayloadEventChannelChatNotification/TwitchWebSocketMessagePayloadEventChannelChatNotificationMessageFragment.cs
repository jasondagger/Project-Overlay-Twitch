
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
	public sealed class TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragment
	{
		public enum FragmentType : uint
		{
			Text = 0u,
			Cheermote,
			Emote,
			Mention,
		}

        [JsonPropertyName(name: "cheermote")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentCheermote Cheermote { get; set; } = null;

        [JsonPropertyName(name: "emote")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote Emote { get; set; } = null;

        [JsonPropertyName(name: "mention")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentMention Mention { get; set; } = null;

        [JsonPropertyName(name: "text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName(name: "type")]
        public string Type { get; set; } = string.Empty;

		public FragmentType GetFragmentType()
		{
            return Type switch
            {
                "cheermote" => 
					FragmentType.Cheermote,

                "emote" => 
					FragmentType.Emote,

                "mention" => 
					FragmentType.Mention,

                _ => 
					FragmentType.Text,
            };
        }
	}
}