using Newtonsoft.Json;

namespace Simva.Model
{
    public class VersionFeatures
    {
        [JsonProperty("simlets")]
        public bool Simlets { get; set; }

        [JsonProperty("sessions")]
        public bool Sessions { get; set; }

        [JsonProperty("lrs_prefix")]
        public bool LrsPrefix { get; set; }
    }

    public class VersionInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("features")]
        public VersionFeatures Features { get; set; }
    }
}
