using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

class SimpleHttpServer
{
    private HttpListener listener;
    private string url;

    public SimpleHttpServer(string url)
    {
        this.url = url;
        listener = new HttpListener();
        listener.Prefixes.Add(url);
    }

    public async Task StartAsync()
    {
        listener.Start();
        Console.WriteLine($"Server listening on {url}");

        while (true)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                ProcessRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        Console.WriteLine($"Request received: {request.Url.PathAndQuery}");

        string responseString = "<html><body><h1>Hello from the C# HTTP Server!</h1></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    public void Stop()
    {
        listener.Stop();
        listener.Close();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        SimpleHttpServer server = new SimpleHttpServer("http://localhost:8080/");
        await server.StartAsync();
    }
}