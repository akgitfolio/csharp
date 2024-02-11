using System.Collections.Concurrent;

class ConcurrentDownloader
{
    private readonly string _url;
    private readonly string _outputPath;
    private readonly int _numThreads;
    private readonly int _chunkSize;

    public ConcurrentDownloader(string url, string outputPath, int numThreads = 8, int chunkSize = 1024 * 1024)
    {
        _url = url;
        _outputPath = outputPath;
        _numThreads = numThreads;
        _chunkSize = chunkSize;
    }

    public void DownloadFile()
    {
        using (var client = new HttpClient())
        {
            var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, _url)).Result;
            var fileSize = response.Content.Headers.ContentLength.Value;
            var chunks = (int)Math.Ceiling((double)fileSize / _chunkSize);

            Console.WriteLine($"File size: {fileSize} bytes");
            Console.WriteLine($"Number of chunks: {chunks}");

            var downloadedChunks = new ConcurrentDictionary<int, byte[]>();
            var threads = new Thread[_numThreads];

            for (int i = 0; i < _numThreads; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() => DownloadChunks(downloadedChunks, chunks, threadId));
                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("All chunks downloaded. Combining...");

            using (var outputStream = File.Create(_outputPath))
            {
                for (int i = 0; i < chunks; i++)
                {
                    outputStream.Write(downloadedChunks[i], 0, downloadedChunks[i].Length);
                }
            }

            Console.WriteLine("Download complete!");
        }
    }

    private void DownloadChunks(ConcurrentDictionary<int, byte[]> downloadedChunks, int totalChunks, int threadId)
    {
        using (var client = new HttpClient())
        {
            while (true)
            {
                var chunkIndex = downloadedChunks.Count;
                if (chunkIndex >= totalChunks)
                    break;

                var start = chunkIndex * _chunkSize;
                var end = Math.Min(start + _chunkSize - 1, (long)totalChunks * _chunkSize - 1);

                var request = new HttpRequestMessage(HttpMethod.Get, _url);
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

                var response = client.SendAsync(request).Result;
                var chunk = response.Content.ReadAsByteArrayAsync().Result;

                downloadedChunks[chunkIndex] = chunk;

                Console.WriteLine($"Thread {threadId}: Downloaded chunk {chunkIndex + 1}/{totalChunks}");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var url = "https://nodejs.org/download/release/v16.20.2/node-v16.20.2.tar.gz";
        string projectRoot =  Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        Console.WriteLine("projectRoot: " + projectRoot);
        var outputPath = Path.Combine(projectRoot, "node-v16.20.2.tar.gz");

        var downloader = new ConcurrentDownloader(url, outputPath);
        downloader.DownloadFile();
    }
}
