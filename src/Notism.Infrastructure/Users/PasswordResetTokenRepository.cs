using Microsoft.EntityFrameworkCore;

using Notism.Domain.User;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Users;

public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<int> DeleteExpiredTokensAsync(DateTime cutoffDate)
    {
        return await _dbSet
            .Where(prt => prt.ExpiresAt < cutoffDate || prt.IsUsed)
            .ExecuteDeleteAsync();
    }
}