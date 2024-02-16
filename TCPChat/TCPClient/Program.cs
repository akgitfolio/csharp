using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Client
{
    static async Task Main()
    {
        using TcpClient client = new TcpClient();
        await client.ConnectAsync("localhost", 8888);
        Console.WriteLine("Connected to server");

        using NetworkStream stream = client.GetStream();

        while (true)
        {
            Console.Write("Enter message (or 'exit' to quit): ");
            string message = Console.ReadLine();
            
            if (message.ToLower() == "exit") break;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");
        }
    }
}