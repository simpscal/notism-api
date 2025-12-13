using Microsoft.Extensions.DependencyInjection;

using Notism.Worker.Interfaces;
using Notism.Worker.Workers.Base;

namespace Notism.Worker.Workers;

public class TokenCleanupWorker : BaseWorker
{
    public TokenCleanupWorker(
        ILogger<TokenCleanupWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
        : base(logger, serviceScopeFactory, configuration)
    {
    }

    protected override string WorkerName => "Token Cleanup Worker";

    protected override TimeSpan GetExecutionInterval()
    {
        var cleanupInterval = _configuration.GetValue<int>("TokenCleanup:IntervalInHours", 168);
        return TimeSpan.FromHours(cleanupInterval);
    }

    protected override async Task ExecuteWorkAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var tokenCleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();
        await tokenCleanupService.CleanupExpiredTokensAsync(cancellationToken);
    }
}