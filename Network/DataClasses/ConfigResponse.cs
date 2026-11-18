using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class ConfigResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("config")]
        public ConfigData Config { get; set; }

        [JsonProperty("network")]
        public NetworkData Network { get; set; }

        [JsonProperty("hardware")]
        public HardwareData Hardware { get; set; }
    }
}
