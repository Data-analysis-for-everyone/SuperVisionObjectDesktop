using Newtonsoft.Json;
using SuperVisionObjectDesktop.Devices;
using SuperVisionObjectDesktop.MessageHandler.Interfaces;
using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network
{
    public class SuperVisionObjectFinder : IMessageNotifiable
    {
        private const string CONFIG_PATH = "/api/token";

        public event EventHandler MessageReceived;

        public List<IDevice>? Devices { get; private set; }

        public SuperVisionObjectFinder()
        {

        }

        public async Task InitComponents()
        {
            Devices = await RunDeviceSearch();
        }

        public async Task<List<IDevice>?> RunDeviceSearch()
        {
            Devices = new List<IDevice>();
            var localIPs = GetLocalIPAddresses();

            if (localIPs.Count == 0)
            {
                MessageBox.Show("Не найдено активных сетевых интерфейсов!", "Ошибка поиска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            string smessage = $"Найдено {localIPs.Count} сетевых интерфейсов:\n";

            foreach (var ip in localIPs)
                smessage += ip + "\n";
            

            DialogMessage message = new DialogMessage() { Message = smessage, MessageType = MessageType.INFO, Method = nameof(RunDeviceSearch) };
            Notify(message);

            // Сканируем каждый интерфейс
            var allDevices = new List<IDevice>();

            foreach (var localIP in localIPs)
            {
                message.Message += $"Сканирование подсети: {localIP}...";
                Notify(message);

                var devices = await ScanSubnet(localIP);

                allDevices.AddRange(devices);

                string findStr = "";
                if (devices.Count > 0)
                {
                    findStr = $"Найдено {devices.Count} устройств ESP32\n";
                    foreach (var dev in devices)
                        findStr += $"{dev.IPAddress} - {dev.Name} \n";
                }
                else
                {
                    findStr = $"Устройства не найдены";
                }
                Notify(message);
            }
            return allDevices;
        }

        private List<IPAddress> GetLocalIPAddresses()
        {
            var addresses = new List<IPAddress>();

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        addresses.Add(ip);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка получения IP адресов", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return addresses;
        }

        private async Task<List<IDevice>> ScanSubnet(IPAddress localIP)
        {
            var devices = new List<IDevice>();
            var subnetMask = GetSubnetMask(localIP);
            if (subnetMask == null)
                subnetMask = IPAddress.Parse("255.255.255.0");
            var baseAddress = GetNetworkAddress(localIP, subnetMask);
            var hostCount = GetHostCount(subnetMask);

            var tasks = new List<Task<IDevice?>>();
            var semaphore = new SemaphoreSlim(20); // 20 параллельных запросов

            for (int i = 1; i < hostCount - 1; i++)
            {
                var ip = GetIPFromNetwork(baseAddress, i);
                if (ip.Equals(localIP))
                    continue;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await ProbeDevice(ip);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            var results = await Task.WhenAll(tasks);
            foreach (var device in results)
            {
                if (device != null)
                {
                    devices.Add(device);
                }
            }
            return devices;
        }

        static async Task<IDevice?> ProbeDevice(IPAddress ip)
        {
            var stopwatch = Stopwatch.StartNew();
            TcpClient? client = null;

            try
            {
                client = new TcpClient();

                // для пустых IP - таймаут 150 мс
                var connectTask = client.ConnectAsync(ip.ToString(), 80);
                if (await Task.WhenAny(connectTask, Task.Delay(150)) != connectTask)
                {
                    return null;
                }
                await connectTask;
                // Время на подключение
                await Task.Delay(40);

                using (var stream = client.GetStream())
                {
                    string request = $"GET {CONFIG_PATH} HTTP/1.0\r\nHost: {ip}\r\nConnection: close\r\n\r\n";
                    byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    await Task.Delay(100);

                    // Читаем с таймаутом 300 мс
                    byte[] buffer = new byte[2048];
                    int totalBytes = 0;

                    using (var cts = new System.Threading.CancellationTokenSource(300))
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            if (!stream.DataAvailable && totalBytes > 0)
                                break;

                            if (!stream.DataAvailable)
                            {
                                await Task.Delay(20);
                                continue;
                            }

                            int read = await stream.ReadAsync(buffer, totalBytes, buffer.Length - totalBytes);
                            if (read == 0) break;
                            totalBytes += read;

                            if (totalBytes >= buffer.Length)
                            {
                                Array.Resize(ref buffer, buffer.Length * 2);
                            }
                        }
                    }

                    if (totalBytes == 0)
                    {
                        return null;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, totalBytes);

                    if (response.Contains("token") || response.Contains("200 OK"))
                    {
                        stopwatch.Stop();

                        int start = response.IndexOf('{');
                        int end = response.LastIndexOf('}');
                        if (start != -1 && end != -1)
                        {
                            string json = response.Substring(start, end - start + 1);
                            try
                            {
                                var tokenData = JsonConvert.DeserializeObject<TokenResponse>(json);
                                if (tokenData != null && !string.IsNullOrEmpty(tokenData.Token))
                                {
                                    var config = await GetDeviceConfig(ip);
                                    return DeviceFabrica.BuildDevice(config);
                                }
                            }
                            catch
                            {
                                return DeviceFabrica.BuildDevice(null);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            finally
            {
                if (client != null)
                {
                    try { client.Close(); } catch { }
                    try { client.Dispose(); } catch { }
                }
            }

            return null;
        }

        static async Task<DeviceConfig?> GetDeviceConfig(IPAddress ip)
        {
            TcpClient? client = null;

            try
            {
                client = new TcpClient();

                var connectTask = client.ConnectAsync(ip.ToString(), 80);
                if (await Task.WhenAny(connectTask, Task.Delay(200)) != connectTask)
                {
                    return null;
                }

                await connectTask;
                await Task.Delay(20);

                using (var stream = client.GetStream())
                {
                    string request = $"GET /api/config HTTP/1.0\r\nHost: {ip}\r\nConnection: close\r\n\r\n";
                    byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    await Task.Delay(50);

                    byte[] buffer = new byte[2048];
                    int totalBytes = 0;

                    using (var cts = new System.Threading.CancellationTokenSource(300))
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            if (!stream.DataAvailable && totalBytes > 0)
                                break;

                            if (!stream.DataAvailable)
                            {
                                await Task.Delay(20);
                                continue;
                            }

                            int read = await stream.ReadAsync(buffer, totalBytes, buffer.Length - totalBytes);
                            if (read == 0) break;
                            totalBytes += read;
                        }
                    }

                    if (totalBytes == 0)
                    {
                        return null;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, totalBytes);

                    int start = response.IndexOf('{');
                    int end = response.LastIndexOf('}');
                    if (start != -1 && end != -1)
                    {
                        string json = response.Substring(start, end - start + 1);
                        var config = JsonConvert.DeserializeObject<ConfigResponse>(json);

                        return new DeviceConfig
                        {
                            Name = config?.Config?.DeviceName ?? "Unknown",
                            Mac = config?.Config?.MacAddress ?? "Unknown",
                            RSSI = config?.Network?.RSSI ?? 0,
                            ChannelsCount = config?.Config?.ChannelsCount ?? 0,
                            DeviceType = config?.Config?.DeviceType ?? "Unknown",
                            FirmwareVersion = config?.Config?.FirmwareVersion ?? "Unknown",
                            SampleFrequancy = config?.Config?.SampleFrequancy ?? 0,
                            SensorType = config?.Config?.SensorType ?? "Unknown",
                            IPAddress = ip
                        };
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            finally
            {
                if (client != null)
                {
                    try { client.Close(); } catch { }
                    try { client.Dispose(); } catch { }
                }
            }

            return null;
        }

        private IPAddress? GetSubnetMask(IPAddress address)
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in networkInterfaces)
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    var properties = ni.GetIPProperties();
                    foreach (var uni in properties.UnicastAddresses)
                    {
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork &&
                            uni.Address.Equals(address))
                        {
                            return uni.IPv4Mask;
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return null;
        }

        private IPAddress GetNetworkAddress(IPAddress address, IPAddress mask)
        {
            var addressBytes = address.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var networkBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(addressBytes[i] & maskBytes[i]);
            }

            return new IPAddress(networkBytes);
        }

        private int GetHostCount(IPAddress mask)
        {
            var maskBytes = mask.GetAddressBytes();
            int bitCount = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((maskBytes[i] & (1 << j)) != 0)
                    {
                        bitCount++;
                    }
                }
            }

            return (int)Math.Pow(2, 32 - bitCount);
        }

        private IPAddress GetIPFromNetwork(IPAddress network, int hostIndex)
        {
            var networkBytes = network.GetAddressBytes();
            var ipBytes = new byte[4];

            Array.Copy(networkBytes, ipBytes, 4);
            ipBytes[3] = (byte)hostIndex;

            return new IPAddress(ipBytes);
        }

        public void Notify(DialogMessage dialogMessage)
        {
            OnNotificationOccurred(dialogMessage);
        }

        protected virtual void OnNotificationOccurred(DialogMessage e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }
}
