using Microsoft.Extensions.Logging;

using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Worker.Interfaces;

namespace Notism.Worker.Services;

public class TokenCleanupService : ITokenCleanupService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        ILogger<TokenCleanupService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _logger = logger;
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting token cleanup process at {Timestamp}", DateTime.UtcNow);

        try
        {
            await CleanupExpiredRefreshTokensAsync(cancellationToken);
            await CleanupExpiredPasswordResetTokensAsync(cancellationToken);

            _logger.LogInformation("Token cleanup process completed successfully at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token cleanup process");
            throw;
        }
    }

    private async Task CleanupExpiredRefreshTokensAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up expired refresh tokens");

        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var deletedCount = await _refreshTokenRepository.DeleteExpiredTokensAsync(oneWeekAgo);

        if (deletedCount > 0)
        {
            _logger.LogInformation("Successfully deleted {Count} expired/revoked refresh tokens", deletedCount);
        }
        else
        {
            _logger.LogInformation("No expired refresh tokens found for cleanup");
        }
    }

    private async Task CleanupExpiredPasswordResetTokensAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up expired password reset tokens");

        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var deletedCount = await _passwordResetTokenRepository.DeleteExpiredTokensAsync(oneWeekAgo);

        if (deletedCount > 0)
        {
            _logger.LogInformation("Successfully deleted {Count} expired/used password reset tokens", deletedCount);
        }
        else
        {
            _logger.LogInformation("No expired password reset tokens found for cleanup");
        }
    }
}