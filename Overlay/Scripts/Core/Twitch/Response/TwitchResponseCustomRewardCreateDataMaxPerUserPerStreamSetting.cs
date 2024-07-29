
namespace Overlay
{
    using System;
    using System.Text.Json.Serialization;

    [Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting
    {
        [JsonPropertyName(name: "is_enabled")]
        public bool IsEnabled = false;

        [JsonPropertyName(name: "max_per_user_per_stream")]
        public long MaxPerUserPerStream = 0;
    }
}