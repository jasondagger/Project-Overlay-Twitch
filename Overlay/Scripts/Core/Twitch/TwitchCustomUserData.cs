
namespace Overlay
{
    using System;

    [Serializable]
    public sealed class TwitchCustomUserData
    {
        public static readonly float TimeStampDelay = 0.5f;

        public string CustomTextColor { get; set; } = string.Empty;
        public string TimeStampSongRequestIsAvailable { get; set; } = $"{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}";
        public string TimeStampTextToSpeechIsAvailable { get; set; } = $"{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}";
    }
}