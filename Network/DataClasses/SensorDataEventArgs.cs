using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public class SensorDataEventArgs : EventArgs
    {
        public long Timestamp { get; set; }
        public ushort Channels { get; set; }
        public int Count { get; set; }
        public List<List<uint>>? Data { get; set; }
    }
}
