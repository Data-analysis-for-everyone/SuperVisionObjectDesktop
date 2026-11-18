using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class SensorValues
    {
        [JsonProperty("temperature")]
        public float Temperature { get; set; }

        [JsonProperty("humidity")]
        public float Humidity { get; set; }

        [JsonProperty("pressure")]
        public float Pressure { get; set; }

        [JsonProperty("rssi")]
        public int Rssi { get; set; }

        [JsonProperty("free_heap")]
        public int FreeHeap { get; set; }

        [JsonProperty("history")]
        public List<float> History { get; set; }
    }
}
