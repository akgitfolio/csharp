using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;

class PacketSniffer
{
    public static void Main(string[] args)
    {
        // Retrieve the device list
        var devices = CaptureDeviceList.Instance;

        // If no devices were found print an error
        if (devices.Count < 1)
        {
            Console.WriteLine("No devices were found on this machine");
            return;
        }

        Console.WriteLine("The following devices are available on this machine:");
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine();

        int i = 0;

        // Print out the devices
        foreach (var dev in devices)
        {
            Console.WriteLine("{0}) {1} {2}", i, dev.Name, dev.Description);
            i++;
        }

        Console.WriteLine();
        Console.Write("-- Please choose a device to capture: ");
        if (!int.TryParse(Console.ReadLine(), out int selectedIndex))
        {
            Console.WriteLine("Invalid input. Exiting.");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= devices.Count)
        {
            Console.WriteLine("Invalid device index. Exiting.");
            return;
        }

        var device = devices[selectedIndex];

        // Register our handler function to the 'packet arrival' event
        device.OnPacketArrival += device_OnPacketArrival;

        // Open the device for capturing
        int readTimeoutMilliseconds = 1000;
        device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);

        Console.WriteLine();
        Console.WriteLine("-- Listening on {0}, hit 'Enter' to stop...",
            device.Description);

        // Start the capturing process
        device.StartCapture();

        // Wait for 'Enter' from the user.
        Console.ReadLine();

        // Stop the capturing process
        device.StopCapture();

        // Close the device
        device.Close();
    }

    private static void device_OnPacketArrival(object sender, PacketCapture e)
    {
        var time = e.Header.Timeval.Date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var len = e.Data.Length;

        var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.Data.ToArray());

        var ipPacket = packet.Extract<IPPacket>();
        if (ipPacket != null)
        {
            var srcIp = ipPacket.SourceAddress;
            var dstIp = ipPacket.DestinationAddress;

            Console.WriteLine("{0} Len={1} {2} -> {3}", time, len, srcIp, dstIp);

            var tcpPacket = packet.Extract<TcpPacket>();
            if (tcpPacket != null)
            {
                var srcPort = tcpPacket.SourcePort;
                var dstPort = tcpPacket.DestinationPort;
                Console.WriteLine("TCP: {0} -> {1}", srcPort, dstPort);
            }

            var udpPacket = packet.Extract<UdpPacket>();
            if (udpPacket != null)
            {
                var srcPort = udpPacket.SourcePort;
                var dstPort = udpPacket.DestinationPort;
                Console.WriteLine("UDP: {0} -> {1}", srcPort, dstPort);
            }
        }
    }
}