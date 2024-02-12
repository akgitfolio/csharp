using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        string parentDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string sourceDirectory = $"{parentDir}/";
        string compressedFile = Path.Join(parentDir, "compressed.zip");
        string extractDirectory = Path.Join(parentDir, "extracted");

        // Compress
        CompressDirectory(sourceDirectory, compressedFile);
        Console.WriteLine($"Directory compressed to {compressedFile}");

        // Decompress with retry
        DecompressToDirectoryWithRetry(compressedFile, extractDirectory);
        Console.WriteLine($"File decompressed to {extractDirectory}");

        // Add file to existing archive
        string newFile = @"C:\NewFile.txt";
        AddFileToArchive(compressedFile, newFile);
        Console.WriteLine($"Added {newFile} to archive");

        // List contents of archive
        ListArchiveContents(compressedFile);
    }

    static void CompressDirectory(string sourceDirectory, string destinationArchiveFileName)
    {
        ZipFile.CreateFromDirectory(sourceDirectory, destinationArchiveFileName, CompressionLevel.Optimal, false);
    }

    static void DecompressToDirectoryWithRetry(string sourceArchiveFileName, string destinationDirectoryName, int maxRetries = 5, int retryDelay = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, true);
                return; // Successful decompression, exit the method
            }
            catch (IOException ex) when (ex.Message.Contains("because it is being used by another process"))
            {
                if (i == maxRetries - 1)
                {
                    throw; // Rethrow the exception if we've exhausted all retries
                }
                Console.WriteLine($"Decompression attempt {i + 1} failed. Retrying in {retryDelay / 1000} seconds...");
                Thread.Sleep(retryDelay);
            }
        }
    }

    static void AddFileToArchive(string archiveFileName, string fileToAdd)
    {
        using (ZipArchive archive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update))
        {
            archive.CreateEntryFromFile(fileToAdd, Path.GetFileName(fileToAdd));
        }
    }

    static void ListArchiveContents(string archiveFileName)
    {
        using (ZipArchive archive = ZipFile.OpenRead(archiveFileName))
        {
            Console.WriteLine("Archive contents:");
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                Console.WriteLine($" - {entry.FullName}");
            }
        }
    }
}
