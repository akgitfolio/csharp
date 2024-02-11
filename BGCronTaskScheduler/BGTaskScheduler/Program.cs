using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;

public class CronBackgroundService : BackgroundService
{
    private readonly CrontabSchedule _schedule;
    private DateTime _nextRun;

    private readonly string _cronExpression;
    private readonly Func<Task> _task;

    public CronBackgroundService(string cronExpression, Func<Task> task)
    {
        _cronExpression = cronExpression;
        _schedule = CrontabSchedule.Parse(cronExpression);
        _task = task;
        _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            var now = DateTime.Now;
            if (now > _nextRun)
            {
                await _task();
                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
            }
            await Task.Delay(1000, stoppingToken); // 1 second delay
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}

// Usage:
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService(sp => new CronBackgroundService(
                    "* * * * *", // Run every 1 min
                    () => Task.Run(() => Console.WriteLine($"Task run at {DateTime.Now}"))
                ));
            })
            .Build();

        await host.RunAsync();
    }
}