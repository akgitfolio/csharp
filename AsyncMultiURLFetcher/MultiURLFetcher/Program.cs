class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        List<string> urls = new List<string>
        {
            "https://example.com",
            "https://google.com",
            "https://github.com"
            // Add more URLs as needed
        };

        string outputDirectory =  Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        // Directory.CreateDirectory(outputDirectory);

        await DownloadHtmlFilesAsync(urls, outputDirectory);

        Console.WriteLine("Download completed.");
    }

    static async Task DownloadHtmlFilesAsync(List<string> urls, string outputDirectory)
    {
        var tasks = new List<Task>();

        foreach (var url in urls)
        {
            tasks.Add(DownloadAndSaveHtmlAsync(url, outputDirectory));
        }

        await Task.WhenAll(tasks);
    }

    static async Task DownloadAndSaveHtmlAsync(string url, string outputDirectory)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();
            
            string fileName = GetSafeFileName(url) + ".html";
            string filePath = Path.Combine(outputDirectory, fileName);
            
            await File.WriteAllTextAsync(filePath, html);
            Console.WriteLine($"Downloaded: {url}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving file for {url}: {ex.Message}");
        }
    }

    static string GetSafeFileName(string url)
    {
        return string.Join("_", new Uri(url).Host.Split(Path.GetInvalidFileNameChars()));
    }
}