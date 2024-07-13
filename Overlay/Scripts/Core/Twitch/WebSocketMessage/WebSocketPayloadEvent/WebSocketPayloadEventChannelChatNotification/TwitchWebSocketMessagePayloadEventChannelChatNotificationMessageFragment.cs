
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

        [JsonPropertyName("cheermote")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentCheermote Cheermote = new();

        [JsonPropertyName("emote")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentEmote Emote = new();

        [JsonPropertyName("mention")]
        public TwitchWebSocketMessagePayloadEventChannelChatNotificationMessageFragmentMention Mention = new();

        [JsonPropertyName("text")]
        public string Text = string.Empty;

        [JsonPropertyName("type")]
        public string Type = string.Empty;

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