using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class ESP32Device
    {
        public string IPAddress { get; set; }
        public string Token { get; set; }
        public long ResponseTime { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public int RSSI { get; set; }
        public string ChipModel { get; set; }
        public int FreeHeap { get; set; }
    }
}
