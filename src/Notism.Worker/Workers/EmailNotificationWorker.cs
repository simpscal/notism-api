using Microsoft.Extensions.DependencyInjection;

using Notism.Worker.Workers.Base;

namespace Notism.Worker.Workers;

/// <summary>
/// Example worker for future email notification functionality.
/// This demonstrates how easy it is to add new workers using the BaseWorker pattern.
/// </summary>
public class EmailNotificationWorker : BaseWorker
{
    public EmailNotificationWorker(
        ILogger<EmailNotificationWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
        : base(logger, serviceScopeFactory, configuration)
    {
    }

    protected override string WorkerName => "Email Notification Worker";

    protected override TimeSpan GetExecutionInterval()
    {
        var interval = _configuration.GetValue<int>("EmailNotification:IntervalInMinutes", 30);
        return TimeSpan.FromMinutes(interval);
    }

    protected override async Task ExecuteWorkAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Future implementation:
        // var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        // await emailService.SendPendingNotificationsAsync(cancellationToken);
        _logger.LogInformation("Email notification worker executed (placeholder implementation)");
        await Task.CompletedTask;
    }
}