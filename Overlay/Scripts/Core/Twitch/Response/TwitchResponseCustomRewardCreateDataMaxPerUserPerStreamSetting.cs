
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled = false;

        [JsonPropertyName("max_per_user_per_stream")]
        public long MaxPerUserPerStream = 0;
    }
}