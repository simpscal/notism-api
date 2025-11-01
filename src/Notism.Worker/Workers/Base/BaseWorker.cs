using Microsoft.Extensions.DependencyInjection;

namespace Notism.Worker.Workers.Base;

public abstract class BaseWorker : Microsoft.Extensions.Hosting.BackgroundService
{
    protected readonly ILogger _logger;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly IConfiguration _configuration;

    protected BaseWorker(
        ILogger logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected abstract string WorkerName { get; }
    protected abstract TimeSpan GetExecutionInterval();
    protected abstract Task ExecuteWorkAsync(IServiceScope scope, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = GetExecutionInterval();

        _logger.LogInformation("{WorkerName} started with interval of {Interval}", WorkerName, interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await ExecuteWorkAsync(scope, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during {WorkerName} execution", WorkerName);
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("{WorkerName} stopped", WorkerName);
    }
}