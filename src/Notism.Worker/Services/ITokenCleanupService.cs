namespace Notism.Worker.Services;

public interface ITokenCleanupService
{
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}