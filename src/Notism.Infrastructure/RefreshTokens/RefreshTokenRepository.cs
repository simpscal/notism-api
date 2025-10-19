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

    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        await _dbSet.AddAsync(refreshToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsRevoked, true));
    }
}