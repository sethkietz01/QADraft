public class TimedHostedService : IHostedService, IDisposable
{
    private readonly SnipeItApiClient _snipeItApiClient;
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private Timer _timer;

    public TimedHostedService(ILogger<TimedHostedService> logger, SnipeItApiClient snipeItApiClient)
    {
        _logger = logger;
        _snipeItApiClient = snipeItApiClient;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, 
            TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        try
        {
            var count = Interlocked.Increment(ref executionCount);

            _snipeItApiClient.UpdateXml();

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }
        catch
        {

        }
        

        
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}