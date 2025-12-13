using Microsoft.EntityFrameworkCore;

using Notism.Domain.RefreshToken;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.RefreshTokens;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsRevoked, true));
    }

    public async Task<int> DeleteExpiredTokensAsync(DateTime cutoffDate)
    {
        return await _dbSet
            .Where(rt => rt.ExpiresAt < cutoffDate || rt.IsRevoked)
            .ExecuteDeleteAsync();
    }
}