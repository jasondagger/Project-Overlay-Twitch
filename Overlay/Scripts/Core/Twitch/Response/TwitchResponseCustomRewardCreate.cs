
namespace Overlay
{
    using System.Text.Json.Serialization;

    [System.Serializable]
    public sealed class TwitchResponseCustomRewardCreate
    {
        [JsonPropertyName("data")]
        public TwitchResponseCustomRewardCreateData[] Data = null;
    }
}