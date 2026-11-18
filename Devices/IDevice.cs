using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Devices
{
    public interface IDevice
    {
        public event EventHandler<SensorDataEventArgs> DataReceived;
        public void Notify(SensorDataEventArgs sensorData);

        public string Name { get; }
        public string Mac { get; }
        public int RSSI { get; }
        public string DeviceType { get;}
        public string SensorType { get; }
        public ushort SampleFrequancy { get; }
        public byte ChannelsCount { get; }
        public string FirmwareVersion { get; }
        public IPAddress IPAddress { get; }
        public bool IsUsing { get; set; }
    }
}
