using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Devices
{
    public class MAX30105 : IDevice
    {
        public string Name { get; private set; }

        public string Mac { get; private set; }

        public int RSSI { get; private set; }

        public string DeviceType { get; private set; }

        public string SensorType { get; private set; }

        public ushort SampleFrequancy { get; private set; }

        public byte ChannelsCount { get; private set; }

        public string FirmwareVersion { get; private set; }

        public IPAddress IPAddress { get; private set; }
        public bool IsUsing { get; set; }

        public MAX30105(DeviceConfig config)
        {
            Name = config.Name;
            Mac = config.Mac;
            RSSI = config.RSSI;
            DeviceType = config.DeviceType;
            SensorType = config.SensorType;
            SampleFrequancy = config.SampleFrequancy;
            ChannelsCount = config.ChannelsCount;
            FirmwareVersion = config.FirmwareVersion;
            IPAddress = config.IPAddress;
            IsUsing = false;
        }

        public event EventHandler<SensorDataEventArgs> DataReceived;

        public void Notify(SensorDataEventArgs sensorData)
        {
            OnNotificationOccurred(sensorData);
        }

        protected virtual void OnNotificationOccurred(SensorDataEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }
    }
}
