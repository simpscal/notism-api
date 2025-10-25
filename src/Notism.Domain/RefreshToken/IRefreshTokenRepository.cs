using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.RefreshToken;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task RevokeAllUserTokensAsync(Guid userId);
}