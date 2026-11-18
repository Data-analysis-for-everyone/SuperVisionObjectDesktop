using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class DeviceConfig
    {
        public string Name { get; set; }
        public string Mac { get; set; }
        public int RSSI { get; set; }
        public string DeviceType { get; set; }
        public string SensorType { get; set; }
        public ushort SampleFrequancy { get; set; }
        public byte ChannelsCount { get; set; }
        public string FirmwareVersion { get; set; }
        public IPAddress IPAddress { get; set; }
    }
}
