using System;
using System.IO;

class Program
{
    static void Main()
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        
        Console.WriteLine("Directory structure:");
        PrintDirectoryTree(projectDirectory, "");
        Console.WriteLine("\nStarting file system watcher...\n");

        using (FileSystemWatcher watcher = new FileSystemWatcher(projectDirectory))
        {
            watcher.IncludeSubdirectories = true;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;
            
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press 'q' to quit.");
            while (Console.Read() != 'q') ;
        }
    }

    private static void PrintDirectoryTree(string directory, string indent)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            Console.WriteLine($"{indent}├─ {dir.Name}");

            FileInfo[] files = dir.GetFiles();
            DirectoryInfo[] subDirs = dir.GetDirectories();

            for (int i = 0; i < files.Length; i++)
            {
                string fileIndent = (i == files.Length - 1 && subDirs.Length == 0) ? "│  └─ " : "│  ├─ ";
                Console.WriteLine($"{indent}{fileIndent}{files[i].Name}");
            }

            for (int i = 0; i < subDirs.Length; i++)
            {
                string subIndent = (i == subDirs.Length - 1) ? "   " : "│  ";
                PrintDirectoryTree(subDirs[i].FullName, indent + subIndent);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"{indent}├─ [Access Denied]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{indent}├─ [Error: {ex.Message}]");
        }
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Created: {e.FullPath}");
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Deleted: {e.FullPath}");
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Changed: {e.FullPath}");
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Renamed: {e.OldFullPath} to {e.FullPath}");
    }
}
