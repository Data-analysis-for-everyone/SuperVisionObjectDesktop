using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class HardwareData
    {
        [JsonProperty("chip_model")]
        public string ChipModel { get; set; }

        [JsonProperty("chip_cores")]
        public int ChipCores { get; set; }

        [JsonProperty("free_heap")]
        public int FreeHeap { get; set; }

        [JsonProperty("flash_size")]
        public int FlashSize { get; set; }
    }
}
