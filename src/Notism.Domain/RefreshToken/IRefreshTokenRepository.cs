using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.RefreshToken;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken> AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId);
}