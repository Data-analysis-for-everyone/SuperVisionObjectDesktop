using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace SuperVisionObjectDesktop._3dParts
{
    /// <summary>
    /// Класс, контролирующий и обеспечивающий взаимодействие с устройствами сбора данных
    /// </summary>
    public class NetworkManager
    {

        string esp_ip = "";
        public NetworkManager() {

        }

        public async Task RunSSE()
        {
            // Настраиваем HttpClient без прокси
            var handler = new HttpClientHandler
            {
                Proxy = null,                    // Отключаем прокси
                UseProxy = false,                // Не использовать прокси
                AutomaticDecompression = DecompressionMethods.None,
                PreAuthenticate = false,
                UseDefaultCredentials = false,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            var request = new HttpRequestMessage(HttpMethod.Get, $"http://{esp_ip}/stream");
            request.Headers.Add("Accept", "text/event-stream");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Connection", "keep-alive");

            try
            {
                Console.WriteLine("⏳ Ожидание ответа от ESP32...");

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);


                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Ошибка: {response.StatusCode} - {response.ReasonPhrase}");
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new System.IO.StreamReader(stream);

                string line;
                string eventType = "";
                string eventData = "";

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("event:"))
                    {
                        eventType = line.Substring(6).Trim();
                    }
                    else if (line.StartsWith("data:"))
                    {
                        eventData = line.Substring(5).Trim();
                    }
                    else if (string.IsNullOrEmpty(line))
                    {
                        if (!string.IsNullOrEmpty(eventData))
                        {
                            ProcessSSEMessage(eventType, eventData);
                        }
                        eventType = "";
                        eventData = "";
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏹️  Соединение прервано по таймауту");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                Console.WriteLine($"   Тип: {ex.GetType().Name}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        static void ProcessSSEMessage(string eventType, string jsonData)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SensorData>(jsonData);

                if (data == null) return;

                if (data.Type == "sensor_data")
                {
                    // Если есть история - выводим первые 5 значений
                    if (data.Data?.History != null && data.Data.History.Count > 0)
                    {
                        var lastValues = data.Data.History.Take(5);
                        Console.WriteLine($"📈  История: {string.Join(", ", lastValues.Select(v => v.ToString("F1")))}");
                        Console.WriteLine(new string('─', 60));
                    }
                }
                else if (data.Type == "init")
                {
                    Console.WriteLine($"📨  {data.Message}");
                    Console.WriteLine(new string('─', 60));
                }
                else
                {
                    Console.WriteLine($"📨  Получено: {jsonData}");
                    Console.WriteLine(new string('─', 60));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга: {ex.Message}");
                Console.WriteLine($"📄  Данные: {jsonData}");
            }
        }



        public async Task RunDeviceSearch()
        {
            // Получаем IP адреса всех активных сетевых интерфейсов
            var localIPs = GetLocalIPAddresses();

            if (localIPs.Count == 0)
            {
                Console.WriteLine("❌ Не найдено активных сетевых интерфейсов!");
                return;
            }

            Console.WriteLine($"🔍 Найдено {localIPs.Count} сетевых интерфейсов:");
            foreach (var ip in localIPs)
            {
                Console.WriteLine($"   • {ip}");
            }
            Console.WriteLine();

            // Сканируем каждый интерфейс
            var allDevices = new List<ESP32Device>();

            foreach (var localIP in localIPs)
            {
                Console.WriteLine($"📡 Сканирование подсети: {localIP}...");
                var stopwatch = Stopwatch.StartNew();
                var devices = await ScanSubnetFast(localIP);
                stopwatch.Stop();
                allDevices.AddRange(devices);

                if (devices.Count > 0)
                {
                    Console.WriteLine($"   ✅ Найдено {devices.Count} устройств ESP32 (за {stopwatch.Elapsed.TotalSeconds:F1} сек)");
                    foreach (var dev in devices)
                    {
                        Console.WriteLine($"      • {dev.IPAddress} - {dev.Name}");
                    }
                }
                else
                {
                    Console.WriteLine($"   ❌ Устройства не найдены (за {stopwatch.Elapsed.TotalSeconds:F1} сек)");
                }
                Console.WriteLine();
            }

            // Вывод результатов
            Console.WriteLine(new string('═', 60));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n📊 ИТОГ: Найдено {allDevices.Count} устройств ESP32");
            Console.ResetColor();

            if (allDevices.Count > 0)
            {
                esp_ip = allDevices[0].IPAddress;

                Console.WriteLine("\n📋 Список устройств:");
                Console.WriteLine(new string('─', 60));

                for (int i = 0; i < allDevices.Count; i++)
                {
                    var dev = allDevices[i];
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  {i + 1}. {dev.Name}");
                    Console.ResetColor();
                    Console.WriteLine($"     IP: {dev.IPAddress}");
                    Console.WriteLine($"     Token: {dev.Token}");
                    Console.WriteLine($"     MAC: {dev.MacAddress}");
                    Console.WriteLine($"     RSSI: {dev.RSSI} dBm");
                    Console.WriteLine($"     Время отклика: {dev.ResponseTime} мс");
                    Console.WriteLine($"     Модель: {dev.ChipModel}");
                    Console.WriteLine($"     Свободно RAM: {dev.FreeHeap} байт");
                    Console.WriteLine(new string('─', 60));
                }

                // Сохраняем найденные IP в файл
                var ips = new List<string>();
                foreach (var dev in allDevices)
                {
                    ips.Add($"http://{dev.IPAddress}/");
                    ips.Add($"http://{dev.IPAddress}/api/token");
                }
                System.IO.File.WriteAllLines("found_esp.txt", ips);
                Console.WriteLine("\n💾 Список IP сохранен в found_esp.txt");
            }



            int s = 0;
        }

        static List<IPAddress> GetLocalIPAddresses()
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
                Console.WriteLine($"Ошибка получения IP адресов: {ex.Message}");
            }

            return addresses;
        }

        static async Task<List<ESP32Device>> ScanSubnetFast(IPAddress localIP)
        {
            var devices = new List<ESP32Device>();

            // Получаем маску подсети
            var subnetMask = GetSubnetMask(localIP);
            if (subnetMask == null)
            {
                subnetMask = IPAddress.Parse("255.255.255.0");
            }

            var baseAddress = GetNetworkAddress(localIP, subnetMask);
            var hostCount = GetHostCount(subnetMask);

            Console.WriteLine($"   • Маска: {subnetMask}");
            Console.WriteLine($"   • Сеть: {baseAddress}");
            Console.WriteLine($"   • Количество хостов: {hostCount}");
            Console.WriteLine($"   • Параллельное сканирование (20 потоков)...");

            var tasks = new List<Task<ESP32Device>>();
            var semaphore = new SemaphoreSlim(20); // 20 параллельных запросов

            // Сканируем все хосты в подсети
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
                        return await ProbeDeviceFast(ip);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            // Ждем завершения всех задач
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

        static async Task<ESP32Device> ProbeDeviceFast(IPAddress ip)
        {
            var stopwatch = Stopwatch.StartNew();
            TcpClient client = null;

            try
            {
                client = new TcpClient();

                // Быстрый таймаут для пустых IP - 150 мс
                var connectTask = client.ConnectAsync(ip.ToString(), 80);
                if (await Task.WhenAny(connectTask, Task.Delay(150)) != connectTask)
                {
                    return null;
                }

                await connectTask;

                // Если подключились - даем чуть больше времени
                await Task.Delay(20);

                using (var stream = client.GetStream())
                {
                    string request = $"GET /api/token HTTP/1.0\r\nHost: {ip}\r\nConnection: close\r\n\r\n";
                    byte[] requestBytes = Encoding.ASCII.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    await Task.Delay(50);

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
                                    // Получаем конфигурацию асинхронно
                                    var config = await GetDeviceConfigFast(ip);

                                    return new ESP32Device
                                    {
                                        IPAddress = ip.ToString(),
                                        Token = tokenData.Token,
                                        ResponseTime = stopwatch.ElapsedMilliseconds,
                                        Name = config?.Name ?? "ESP32-C3",
                                        MacAddress = config?.Mac ?? "Unknown",
                                        RSSI = config?.RSSI ?? 0,
                                        ChipModel = "ESP32-C3",
                                        FreeHeap = 0
                                    };
                                }
                            }
                            catch
                            {
                                return new ESP32Device
                                {
                                    IPAddress = ip.ToString(),
                                    Token = "FOUND",
                                    ResponseTime = stopwatch.ElapsedMilliseconds,
                                    Name = "ESP32-C3",
                                    MacAddress = "Unknown",
                                    RSSI = 0,
                                    ChipModel = "ESP32-C3",
                                    FreeHeap = 0
                                };
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

        static async Task<DeviceConfig> GetDeviceConfigFast(IPAddress ip)
        {
            TcpClient client = null;

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
                            SensorType = config?.Config?.SensorType ?? "Unknown"
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

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

        static IPAddress GetSubnetMask(IPAddress address)
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

        static IPAddress GetNetworkAddress(IPAddress address, IPAddress mask)
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

        static int GetHostCount(IPAddress mask)
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

        static IPAddress GetIPFromNetwork(IPAddress network, int hostIndex)
        {
            var networkBytes = network.GetAddressBytes();
            var ipBytes = new byte[4];

            Array.Copy(networkBytes, ipBytes, 4);
            ipBytes[3] = (byte)hostIndex;

            return new IPAddress(ipBytes);
        }
    }
}
