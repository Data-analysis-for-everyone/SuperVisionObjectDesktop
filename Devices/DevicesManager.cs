using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Devices
{
    /// <summary>
    /// Класс для контроля опроса данных от устройств
    /// </summary>
    public class DevicesManager
    {
        private List<IDevice> _devices;
        private readonly Dictionary<IPAddress, DeviceHandler> _devicesHandler = new();
        private readonly object _lock = new();

        public DevicesManager(List<IDevice> devices)
        {
            _devices = devices;
        }

        public void RunProcess()
        {
            foreach (var device in _devices)
            {
                AddDevice(device);
            }
        }

        public void AddDevice(IDevice device)
        {
            lock (_lock)
            {
                if (_devicesHandler.ContainsKey(device.IPAddress))
                    return;

                var handler = new DeviceHandler(device);
                handler.OnError += (s, ex) => HandleError(device, ex);
                handler.Start();

                _devicesHandler[device.IPAddress] = handler;
            }
        }

        public void RemoveDevice(IDevice device)
        {
            lock (_lock)
            {
                if (_devicesHandler.TryGetValue(device.IPAddress, out var handler))
                {
                    handler.Dispose();
                    _devicesHandler.Remove(device.IPAddress);
                    _devices.Remove(device);
                }
            }
        }

        private void ProcessData(int deviceId, byte[] data)
        {
            // Обработка данных
        }

        private void HandleError(IDevice device, Exception ex)
        {
            // Логирование ошибок
            int s = 0;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var deviceHandler in _devicesHandler.Values)
                    deviceHandler.Dispose();
                _devices.Clear();
            }
        }



    }
}
