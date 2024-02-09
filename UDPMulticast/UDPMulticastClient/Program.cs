using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class MulticastClient
{
    private const string MulticastAddress = "239.0.0.1";
    private const int MulticastPort = 8888;

    static void Main(string[] args)
    {
        IPAddress multicastIP = IPAddress.Parse(MulticastAddress);
        IPEndPoint localEP = new IPEndPoint(IPAddress.Any, MulticastPort);

        using (UdpClient udpClient = new UdpClient())
        {
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEP);
            udpClient.JoinMulticastGroup(multicastIP);

            Console.WriteLine("Listening for multicast messages...");

            while (true)
            {
                IPEndPoint remoteEP = null;
                byte[] data = udpClient.Receive(ref remoteEP);

                // Decode binary data
                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine($"Received: {message} from {remoteEP}");
            }
        }
    }
}