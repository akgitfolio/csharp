// Program.cs
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Newtonsoft.Json.Linq;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand
        {
            new Command("get", "Get a configuration setting")
            {
                new Argument<string>("key", "The configuration key")
            }.Handler = CommandHandler.Create<string>(GetConfig),

            new Command("set", "Set a configuration setting")
            {
                new Argument<string>("key", "The configuration key"),
                new Argument<string>("value", "The configuration value")
            }.Handler = CommandHandler.Create<string, string>(SetConfig)
        };

        return rootCommand.InvokeAsync(args).Result;
    }

    static void GetConfig(string key)
    {
        var config = ConfigurationManager.Configuration[key];
        Console.WriteLine(config ?? "Key not found.");
    }

    static void SetConfig(string key, string value)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var json = File.ReadAllText(filePath);
        var jsonObj = JObject.Parse(json);
        jsonObj[key] = value;
        File.WriteAllText(filePath, jsonObj.ToString());
        Console.WriteLine($"Set {key} to {value}");
    }
}