using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LogAnalyzer
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}";
        }
    }

    public class LogFileAnalyzer
    {
        private List<LogEntry> logEntries = new List<LogEntry>();
        private readonly Regex logPattern = new Regex(@"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[(\w+)\] (.*)");

        public void AnalyzeLogFile(string filePath, string keyword = null)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ParseLine(line, keyword);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"Error: File not found - {filePath}");
            }
            catch (IOException e)
            {
                Console.Error.WriteLine($"Error reading file: {e.Message}");
            }
        }

        private void ParseLine(string line, string keyword)
        {
            Match match = logPattern.Match(line);
            if (match.Success)
            {
                try
                {
                    DateTime timestamp = DateTime.ParseExact(match.Groups[1].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string level = match.Groups[2].Value;
                    string message = match.Groups[3].Value;

                    if (string.IsNullOrEmpty(keyword) || message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        logEntries.Add(new LogEntry { Timestamp = timestamp, Level = level, Message = message });
                    }
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine($"Error parsing date: {match.Groups[1].Value}");
                }
            }
            else
            {
                Console.Error.WriteLine($"Warning: Unparseable log entry: {line}");
            }
        }

        public void PrintEntries()
        {
            foreach (var entry in logEntries)
            {
                Console.WriteLine(entry);
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: LogFileAnalyzer <logfile> [keyword]");
                return;
            }

            LogFileAnalyzer analyzer = new LogFileAnalyzer();
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            string filePath = args[0];
            string keyword = args.Length > 1 ? args[1] : null;

            analyzer.AnalyzeLogFile($"{projectDirectory}/{filePath}", keyword);
            analyzer.PrintEntries();

            Console.WriteLine($"\nTotal entries found: {analyzer.logEntries.Count}");
        }
    }
}
