using Notism.Domain.Common.Repositories;
using Notism.Domain.User;

namespace Notism.Domain.User.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<int> DeleteExpiredTokensAsync(DateTime cutoffDate);
}