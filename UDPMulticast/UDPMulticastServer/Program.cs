using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class MulticastServer
{
    private const string MulticastAddress = "239.0.0.1";
    private const int MulticastPort = 8888;

    static void Main(string[] args)
    {
        IPAddress multicastIP = IPAddress.Parse(MulticastAddress);
        IPEndPoint remoteEP = new IPEndPoint(multicastIP, MulticastPort);

        using (UdpClient udpClient = new UdpClient())
        {
            udpClient.JoinMulticastGroup(multicastIP);

            while (true)
            {
                Console.Write("Enter message to send: ");
                string message = Console.ReadLine();

                // Encode message as binary
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Send data
                udpClient.Send(data, data.Length, remoteEP);

                Console.WriteLine($"Sent: {message}");
            }
        }
    }
}