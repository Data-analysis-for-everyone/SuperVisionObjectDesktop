using SuperVisionObjectDesktop.Devices;
using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.DataHandler
{
    public class DeviceDataHandler : IDisposable
    {
        private IDevice _device;


        public DeviceDataHandler(IDevice device)
        {
            _device = device;
            _device.DataReceived += DataHandle;
        }

        private void DataHandle(object? sender, SensorDataEventArgs e)
        {
            // Обработка данных от сенсора
        }

        public void Dispose()
        {
            _device.DataReceived -= DataHandle;

        }

        
    }
}
