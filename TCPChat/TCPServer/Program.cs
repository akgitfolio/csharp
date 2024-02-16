using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    static async Task Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
        
        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0) break;
                
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");
                
                byte[] response = Encoding.UTF8.GetBytes($"Server received: {message}");
                await stream.WriteAsync(response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
        }
    }
}