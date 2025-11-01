namespace Notism.Worker.Interfaces;

public interface ITokenCleanupService
{
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}