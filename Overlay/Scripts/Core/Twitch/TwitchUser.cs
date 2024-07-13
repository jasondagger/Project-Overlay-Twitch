
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchUser
    {
        public string id = string.Empty;
        public string login = string.Empty;
        public string display_name = string.Empty;
        public string type = string.Empty;
        public string broadcaster_type = string.Empty;
        public string description = string.Empty;
        public string profile_image_url = string.Empty;
        public string offline_image_url = string.Empty;
        public int view_count = 0;
        public string email = string.Empty;
        public string created_at = string.Empty;
    }
}