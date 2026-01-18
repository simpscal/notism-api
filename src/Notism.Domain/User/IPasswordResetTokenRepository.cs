using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.User;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<int> DeleteExpiredTokensAsync(DateTime cutoffDate);
}