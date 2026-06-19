using Notism.Domain.User;
using Notism.Domain.User.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class BankAccountRepository : Repository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}
