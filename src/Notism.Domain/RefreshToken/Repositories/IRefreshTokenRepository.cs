using Notism.Domain.Common.Repositories;
using Notism.Domain.RefreshToken;

namespace Notism.Domain.RefreshToken.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task RevokeAllUserTokensAsync(Guid userId);
    Task<int> DeleteExpiredTokensAsync(DateTime cutoffDate);
}