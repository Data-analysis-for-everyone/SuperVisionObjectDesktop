using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class ConfigData
    {
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }

        [JsonProperty("device_type")]
        public string DeviceType { get; set; }

        [JsonProperty("sensor_type")]
        public string SensorType { get; set; }

        [JsonProperty("base_sample_frequancy")]
        public ushort SampleFrequancy { get; set; }

        [JsonProperty("channels_count")]
        public byte ChannelsCount { get; set; }

        [JsonProperty("firmware_version")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("mac_address")]
        public string MacAddress { get; set; }

        [JsonProperty("ip_address")]
        public string IPAddress { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }
    }
}
