using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class NetworkData
    {
        [JsonProperty("ssid")]
        public string SSID { get; set; }

        [JsonProperty("rssi")]
        public int RSSI { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }
    }
}
