
using System.ComponentModel;
using System.Diagnostics;

class SystemMonitor
{
    static void Main()
    {
        Console.WriteLine("Simple System Monitor");
        Console.WriteLine("Press Ctrl+C to exit");

        // Set up cancellation token to handle Ctrl+C
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                Console.Clear();
                PrintCPUUsage();
                PrintMemoryUsage();
                PrintDiskUsage();
                PrintTopProcesses();
                Thread.Sleep(2000); // Update every 2 seconds
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Monitoring stopped.");
        }
    }

    static void PrintCPUUsage()
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue(); // First call will always return 0
        Thread.Sleep(1000); // Wait a second to get a valid reading
        float cpuUsage = cpuCounter.NextValue();
        Console.WriteLine($"CPU Usage: {cpuUsage:F1}%");
    }

    static void PrintMemoryUsage()
    {
        var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var usedMemory = GC.GetTotalMemory(false);
        float memoryUsagePercentage = ((float)usedMemory / totalMemory) * 100;
        Console.WriteLine($"Memory Usage: {memoryUsagePercentage:F1}% ({FormatBytes(usedMemory)} / {FormatBytes(totalMemory)})");
    }

    static void PrintDiskUsage()
    {
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            foreach (var drive in drives)
            {
                long usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                float usagePercentage = ((float)usedSpace / drive.TotalSize) * 100;
                Console.WriteLine($"Disk {drive.Name} Usage: {usagePercentage:F1}% ({FormatBytes(usedSpace)} / {FormatBytes(drive.TotalSize)})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting disk usage: {ex.Message}");
        }
    }

    static void PrintTopProcesses()
    {
        Console.WriteLine("\nTop Processes by CPU Usage:");
        var processes = Process.GetProcesses()
            .Select(p => new 
            { 
                Process = p, 
                CpuTime = GetSafeTotalProcessorTime(p) 
            })
            .OrderByDescending(x => x.CpuTime)
            .Take(5);

        foreach (var p in processes)
        {
            Console.WriteLine($"{p.Process.ProcessName}: {p.CpuTime.TotalSeconds:F1}s CPU time");
        }
    }

    static TimeSpan GetSafeTotalProcessorTime(Process process)
    {
        try
        {
            return process.TotalProcessorTime;
        }
        catch (Win32Exception)
        {
            // Access denied, return TimeSpan.Zero
            return TimeSpan.Zero;
        }
        catch (InvalidOperationException)
        {
            // Process has exited, return TimeSpan.Zero
            return TimeSpan.Zero;
        }
    }


    static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:F1} {suffixes[counter]}";
    }
}
