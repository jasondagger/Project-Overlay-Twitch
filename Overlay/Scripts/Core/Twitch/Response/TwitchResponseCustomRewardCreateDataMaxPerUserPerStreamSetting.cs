
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseCustomRewardCreateDataMaxPerUserPerStreamSetting
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled = false;

        [JsonPropertyName("max_per_user_per_stream")]
        public long MaxPerUserPerStream = 0;
    }
}