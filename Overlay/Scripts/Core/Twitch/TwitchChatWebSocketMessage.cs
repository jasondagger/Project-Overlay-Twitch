
namespace Overlay
{
    using System.Collections.Generic;

    public struct TwitchChatWebSocketMessage
    {
        public Dictionary<string, string> Tags = new();
        public string UserName = string.Empty;
        public string Command = string.Empty;
        public string Text = string.Empty;

        public TwitchChatWebSocketMessage(
            Dictionary<string, string> tags,
            string userName,
            string command,
            string text
        )
        {
            this.Tags = tags;
            this.UserName = userName;
            this.Command = command;
            this.Text = text;
        }
    };
}