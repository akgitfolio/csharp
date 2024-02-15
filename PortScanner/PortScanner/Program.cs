using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class PortScanner
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter IP address to scan: ");
        string ipAddress = Console.ReadLine();

        Console.Write("Enter start port: ");
        int startPort = int.Parse(Console.ReadLine());

        Console.Write("Enter end port: ");
        int endPort = int.Parse(Console.ReadLine());

        Console.WriteLine($"Scanning {ipAddress} from port {startPort} to {endPort}...");

        var openPorts = new ConcurrentBag<int>();
        int totalPorts = endPort - startPort + 1;
        int scannedPorts = 0;

        await Parallel.ForEachAsync(Enumerable.Range(startPort, totalPorts), new ParallelOptions { MaxDegreeOfParallelism = 100 }, async (port, token) =>
        {
            if (await IsPortOpenAsync(ipAddress, port))
            {
                openPorts.Add(port);
                Console.WriteLine($"Port {port} is open");
            }
            
            scannedPorts++;
            if (scannedPorts % 100 == 0 || scannedPorts == totalPorts)
            {
                Console.WriteLine($"Progress: {scannedPorts}/{totalPorts} ports scanned");
            }
        });

        Console.WriteLine("Scan complete.");
        Console.WriteLine($"Open ports: {string.Join(", ", openPorts.OrderBy(p => p))}");
    }

    static async Task<bool> IsPortOpenAsync(string host, int port)
    {
        using (var client = new TcpClient())
        {
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
                {
                    await client.ConnectAsync(IPAddress.Parse(host), port, cts.Token);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}